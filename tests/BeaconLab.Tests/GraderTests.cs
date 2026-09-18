using BeaconLab.Contracts;
using BeaconLab.Hunt;

public class GraderTests
{
    [Fact]
    public void Grader_flags_only_the_beacon_pair()
    {
        var rows = CampaignExpander.Expand(CampaignSamples.Valid()).Select(o => o.Event);
        var hits = PeriodicityHunt.Flag(rows);
        Assert.Equal(new[] { ("WKSTN-04", "cdn-updates.example") }, hits.Select(h => (h.DeviceName, h.DestinationDomain)).ToArray());
    }
}
