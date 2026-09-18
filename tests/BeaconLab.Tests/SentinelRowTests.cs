using System.Text.Json;
using BeaconLab.Contracts;
using BeaconLab.Sentinel;

public class SentinelRowTests
{
    [Fact]
    public void Sentinel_row_has_TimeGenerated()
    {
        var source = CampaignExpander.Expand(CampaignSamples.Valid())[0].Event;
        using var doc = JsonDocument.Parse(SentinelRow.ToJson(source));
        foreach (var prop in typeof(BeaconEvent).GetProperties())
            Assert.True(doc.RootElement.TryGetProperty(prop.Name, out _), prop.Name);
    }
}
