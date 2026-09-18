// This is the real Function, not a stand-in. Consumer group fn.
// Azurite is the checkpoint store, not the hub. Failed bodies go to container failed-events.
using System.Text.Json;
using Azure.Storage.Blobs;
using BeaconLab.Contracts;
using BeaconLab.Mapping;
using BeaconLab.Sentinel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BeaconLab.Functions;

public sealed class Normalize(ILogger<Normalize> logger)
{
    [Function(nameof(Normalize))]
    [EventHubOutput("normalized-events", Connection = "EventHubConnection")]
    public async Task<string[]> Run(
        [EventHubTrigger("raw-events", Connection = "EventHubConnection", ConsumerGroup = "fn")] string[] events)
    {
        var good = new List<string>();
        foreach (var body in events)
        {
            try
            {
                var mapped = IngestionMapping.Apply(body);
                if (mapped.Row is null || mapped.Failures.Count > 0)
                {
                    await WriteFailureAsync(body, string.Join(",", mapped.Failures));
                    continue;
                }
                good.Add(JsonSerializer.Serialize(mapped.Row));
                await File.AppendAllTextAsync(
                    Path.Combine(SentinelRowDirectory(), mapped.Row.DeviceName + "-" + mapped.Row.TimeGenerated.ToUnixTimeMilliseconds() + ".json"),
                    SentinelRow.ToJson(mapped.Row) + Environment.NewLine);
            }
            catch (Exception ex) when (ex is JsonException or FormatException)
            {
                await WriteFailureAsync(body, ex.Message);
            }
        }
        return good.ToArray();
    }

    private async Task WriteFailureAsync(string body, string reason)
    {
        logger.LogWarning("Bad beacon body: {Reason}", reason);
        var service = new BlobServiceClient(AzuriteConnection());
        var container = service.GetBlobContainerClient("failed-events");
        await container.CreateIfNotExistsAsync();
        await container.GetBlobClient(Guid.NewGuid() + ".json").UploadAsync(BinaryData.FromString(body + "\n" + reason), overwrite: true);
    }

    private static string AzuriteConnection() =>
        "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;";

    private static string SentinelRowDirectory()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "sentinel-rows");
        Directory.CreateDirectory(dir);
        return dir;
    }
}
