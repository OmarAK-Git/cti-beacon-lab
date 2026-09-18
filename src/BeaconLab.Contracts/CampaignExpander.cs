namespace BeaconLab.Contracts;

public static class CampaignExpander
{
    public static readonly DateTimeOffset Start = new(2026, 9, 18, 8, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<OutboundEvent> Expand(Campaign campaign)
    {
        if (campaign.Beacon is null)
            throw new CampaignException("Campaign is missing a beacon.");
        if (campaign.Beacon.JitterPercent is < 0 or > 50)
            throw new CampaignException("jitterPercent must be 0..50.");
        if (campaign.Beacon.Sightings < 10)
            throw new CampaignException("beacon sightings must be at least 10.");

        var list = new List<OutboundEvent>();
        list.AddRange(ExpandBeacon(campaign));
        foreach (var noise in campaign.Noise)
            list.AddRange(ExpandNoise(noise));
        return list;
    }

    private static IEnumerable<OutboundEvent> ExpandBeacon(Campaign campaign)
    {
        var beacon = campaign.Beacon;
        var rng = new Random(campaign.Seed);
        var cursor = Start;
        for (var i = 0; i < beacon.Sightings; i++)
        {
            if (i > 0)
            {
                var jitter = rng.Next(-9, 10);
                cursor = cursor.AddSeconds(beacon.IntervalSeconds + jitter);
            }
            yield return new OutboundEvent(Event(beacon, cursor), beacon.DeviceName);
        }
    }

    private static IEnumerable<OutboundEvent> ExpandNoise(NoiseSource noise)
    {
        var cursor = Start;
        for (var i = 0; i < noise.Sightings; i++)
        {
            if (i > 0)
                cursor = cursor.AddSeconds(noise.GapSeconds[(i - 1) % noise.GapSeconds.Count]);
            yield return new OutboundEvent(Event(noise, cursor), noise.DeviceName);
        }
    }

    private static BeaconEvent Event(BeaconSource source, DateTimeOffset when) => new(
        when, source.DeviceName, source.AccountName, source.ProcessName, source.CommandLine,
        source.Sha256, source.DestinationDomain, source.DestinationIp, source.DestinationPort, source.BytesOut);

    private static BeaconEvent Event(NoiseSource source, DateTimeOffset when) => new(
        when, source.DeviceName, source.AccountName, source.ProcessName, source.CommandLine,
        source.Sha256, source.DestinationDomain, source.DestinationIp, source.DestinationPort, source.BytesOut);
}
