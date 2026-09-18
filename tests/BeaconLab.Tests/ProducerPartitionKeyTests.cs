using BeaconLab.Contracts;
using BeaconLab.Producer;

public class ProducerPartitionKeyTests
{
    [Fact]
    public async Task Producer_partition_key_is_device()
    {
        var publisher = new RecordingPublisher();
        foreach (var outbound in CampaignExpander.Expand(CampaignSamples.Valid()))
            await publisher.PublishAsync(outbound);

        Assert.All(publisher.Sent, sent => Assert.Equal(sent.Event.DeviceName, sent.PartitionKey));
        Assert.Contains(publisher.Sent, sent => sent.PartitionKey == "WKSTN-04");
    }
}
