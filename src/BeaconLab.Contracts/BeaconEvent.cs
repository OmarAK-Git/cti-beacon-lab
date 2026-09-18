namespace BeaconLab.Contracts;

public sealed record BeaconEvent(
    DateTimeOffset TimeGenerated,
    string DeviceName,
    string AccountName,
    string ProcessName,
    string CommandLine,
    string Sha256,
    string DestinationDomain,
    string DestinationIp,
    int DestinationPort,
    int BytesOut);
