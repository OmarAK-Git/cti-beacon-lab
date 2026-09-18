namespace BeaconLab.Contracts;

public sealed record Campaign(int Seed, BeaconSource Beacon, IReadOnlyList<NoiseSource> Noise);

public sealed record BeaconSource(
    string DeviceName,
    string AccountName,
    string ProcessName,
    string CommandLine,
    string Sha256,
    string DestinationDomain,
    string DestinationIp,
    int DestinationPort,
    int BytesOut,
    int IntervalSeconds,
    int JitterPercent,
    int Sightings);

public sealed record NoiseSource(
    string DeviceName,
    string AccountName,
    string ProcessName,
    string CommandLine,
    string Sha256,
    string DestinationDomain,
    string DestinationIp,
    int DestinationPort,
    int BytesOut,
    IReadOnlyList<int> GapSeconds,
    int Sightings);

public sealed class CampaignException(string message) : Exception(message);

public sealed record OutboundEvent(BeaconEvent Event, string PartitionKey);
