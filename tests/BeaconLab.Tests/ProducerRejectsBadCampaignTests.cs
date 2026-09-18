using BeaconLab.Contracts;

public class ProducerRejectsBadCampaignTests
{
    [Fact]
    public void Producer_rejects_bad_campaign()
    {
        var highJitter = CampaignSamples.Valid() with { Beacon = CampaignSamples.Valid().Beacon with { JitterPercent = 51 } };
        var few = CampaignSamples.Valid() with { Beacon = CampaignSamples.Valid().Beacon with { Sightings = 9 } };
        Assert.Throws<CampaignException>(() => CampaignExpander.Expand(highJitter));
        Assert.Throws<CampaignException>(() => CampaignExpander.Expand(few));
        Assert.Throws<CampaignException>(() => CampaignExpander.Expand(CampaignSamples.Valid() with { Beacon = null! }));
    }
}
