using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using BeaconLab.Contracts;

public class NormalizeFailureTests
{
    private const string EventHubConnection =
        "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

    private const string AzuriteConnection =
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    [Fact]
    public async Task Normalize_bad_body_writes_failure_blob_and_continues()
    {
        using (var tcp = new TcpClient())
        {
            using var connectCts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            try
            {
                await tcp.ConnectAsync("127.0.0.1", 5672, connectCts.Token);
            }
            catch
            {
                Assert.Fail("emulator not running");
            }
        }

        var failedEvents = new BlobServiceClient(AzuriteConnection)
            .GetBlobContainerClient("failed-events");
        await failedEvents.CreateIfNotExistsAsync();
        await foreach (var stale in failedEvents.GetBlobsAsync())
            await failedEvents.DeleteBlobIfExistsAsync(stale.Name);

        var runMarker = Guid.NewGuid().ToString("N");
        var beacon = CampaignExpander.Expand(CampaignSamples.Valid())[0].Event
            with { CommandLine = runMarker };
        var goodBody = JsonSerializer.Serialize(beacon);

        await using (var producer = new EventHubProducerClient(EventHubConnection, "raw-events"))
        {
            using var batch = await producer.CreateBatchAsync(new CreateBatchOptions { PartitionKey = "WKSTN-04" });
            Assert.True(batch.TryAdd(new EventData(Encoding.UTF8.GetBytes("not-json"))));
            Assert.True(batch.TryAdd(new EventData(Encoding.UTF8.GetBytes(goodBody))));
            await producer.SendAsync(batch);
        }

        var received = new List<string>();
        await using (var consumer = new EventHubConsumerClient("test-reader", EventHubConnection, "normalized-events"))
        {
            using var readCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                await foreach (var item in consumer.ReadEventsAsync(
                    startReadingAtEarliestEvent: true,
                    cancellationToken: readCts.Token))
                {
                    if (item.Data is null)
                        continue;
                    var body = Encoding.UTF8.GetString(item.Data.EventBody.ToArray());
                    if (!body.Contains(runMarker, StringComparison.Ordinal))
                        continue;
                    received.Add(body);
                }
            }
            catch (OperationCanceledException) when (readCts.IsCancellationRequested)
            {
                // fixed 30s window elapsed; assert on what was collected
            }
        }

        if (received.Count == 0)
            Assert.Fail("function host not running");

        Assert.Single(received);
        Assert.Contains("WKSTN-04", received[0]);

        var found = false;
        await foreach (var blob in failedEvents.GetBlobsAsync())
        {
            var content = (await failedEvents.GetBlobClient(blob.Name).DownloadContentAsync()).Value.Content.ToString();
            if (content.Contains("not-json", StringComparison.Ordinal))
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "expected a failed-events blob containing not-json");
    }
}
