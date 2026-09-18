using System.Text.Json;
using BeaconLab.Contracts;

public class ProducerEventsOmitGroundTruthTests
{
    [Fact]
    public void Producer_events_omit_ground_truth()
    {
        var events = CampaignExpander.Expand(CampaignSamples.Valid());
        Assert.NotEmpty(events);
        foreach (var outbound in events)
        {
            var json = JsonSerializer.Serialize(outbound.Event);
            Assert.DoesNotContain("interval", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("jitter", json, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("isBeacon", json, StringComparison.OrdinalIgnoreCase);
        }
    }
}
