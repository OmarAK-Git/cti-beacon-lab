using System.Text.Json;
using BeaconLab.Contracts;

namespace BeaconLab.Mapping;

// Stands in for: the ADX Event Hubs data connection and JSON mapping.
// Same: Path values. Only at swap-in: the cluster and consumer group adx.
// Not this: a hub reader. This does not join group adx.
public static class IngestionMapping
{
    public static readonly string[] Paths =
    [
        "$.TimeGenerated", "$.DeviceName", "$.AccountName", "$.ProcessName",
        "$.CommandLine", "$.Sha256", "$.DestinationDomain", "$.DestinationIp",
        "$.DestinationPort", "$.BytesOut"
    ];

    public static MappingResult Apply(string json, IReadOnlyList<string>? paths = null)
    {
        paths ??= Paths;
        using var doc = JsonDocument.Parse(json);
        var failures = Paths.Where(path => !paths.Contains(path)).ToList();
        foreach (var path in paths)
        {
            var leaf = path.Replace("$.", "");
            if (!doc.RootElement.TryGetProperty(leaf, out _))
                failures.Add(path);
        }
        if (failures.Count > 0)
            return new MappingResult(null, failures);
        var row = JsonSerializer.Deserialize<BeaconEvent>(json);
        return new MappingResult(row, failures);
    }
}

public sealed record MappingResult(BeaconEvent? Row, IReadOnlyList<string> Failures);
