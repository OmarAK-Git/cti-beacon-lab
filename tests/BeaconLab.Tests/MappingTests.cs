using System.Text.Json;
using BeaconLab.Contracts;
using BeaconLab.Mapping;

public class MappingTests
{
    [Fact]
    public void Mapping_paths_cover_every_beacon_field()
    {
        var props = typeof(BeaconEvent).GetProperties().Select(p => p.Name).ToHashSet();
        var leaves = IngestionMapping.Paths.Select(path => path.Replace("$.", "")).ToHashSet();
        Assert.Equal(props, leaves);
    }

    [Fact]
    public void Mapping_failure_is_recorded()
    {
        var good = CampaignExpander.Expand(CampaignSamples.Valid())[0].Event;
        var json = JsonSerializer.Serialize(good);
        var paths = IngestionMapping.Paths.Where(p => p != "$.BytesOut").ToArray();
        var result = IngestionMapping.Apply(json, paths);
        Assert.Null(result.Row);
        Assert.Contains("$.BytesOut", result.Failures);
    }
}
