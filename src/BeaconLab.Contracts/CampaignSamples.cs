namespace BeaconLab.Contracts;

public static class CampaignSamples
{
    public const string LabSha = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";

    public static Campaign Valid() => new(
        Seed: 1,
        Beacon: new BeaconSource(
            "WKSTN-04", "jdoe", "rundll32.exe",
            @"rundll32.exe C:\ProgramData\sync.dll,Entry",
            LabSha, "cdn-updates.example", "203.0.113.44", 443, 428,
            IntervalSeconds: 60, JitterPercent: 15, Sightings: 10),
        Noise: new[]
        {
            new NoiseSource("WKSTN-11", "ada", "svchost.exe", "svchost.exe", LabSha,
                "noisy.example", "203.0.113.11", 443, 100,
                GapSeconds: new[] { 5, 600 }, Sightings: 12),
            new NoiseSource("WKSTN-12", "ada", "svchost.exe", "svchost.exe", LabSha,
                "sparse.example", "203.0.113.12", 443, 100,
                GapSeconds: new[] { 100, 300 }, Sightings: 3)
        });
}
