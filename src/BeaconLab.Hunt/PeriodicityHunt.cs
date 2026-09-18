using BeaconLab.Contracts;

namespace BeaconLab.Hunt;

// Stands in for: you, checking the hunt. Not ADX. Not Sentinel.
// The campaign interval is not an input. Only the rows are.
public static class PeriodicityHunt
{
    public const int MinGaps = 8;
    public const double MaxCv = 0.25;

    public static IReadOnlyList<HuntHit> Flag(IEnumerable<BeaconEvent> rows)
    {
        var hits = new List<HuntHit>();
        foreach (var group in rows.GroupBy(r => (r.DeviceName, r.DestinationDomain)))
        {
            var times = group.Select(r => r.TimeGenerated).OrderBy(t => t).ToArray();
            var gaps = new List<double>();
            for (var i = 1; i < times.Length; i++)
                gaps.Add((times[i] - times[i - 1]).TotalSeconds);
            if (gaps.Count < MinGaps)
                continue;
            var avg = gaps.Average();
            if (avg <= 0)
                continue;
            var variance = gaps.Sum(g => (g - avg) * (g - avg)) / (gaps.Count - 1);
            var cv = Math.Sqrt(variance) / avg;
            if (cv < MaxCv)
                hits.Add(new HuntHit(group.Key.DeviceName, group.Key.DestinationDomain));
        }
        return hits;
    }
}

public sealed record HuntHit(string DeviceName, string DestinationDomain);
