using System.IO;

namespace BeaconLab.Tests;

public class KqlContractTests
{
    [Fact]
    public void Kql_uses_the_contract_thresholds()
    {
        var root = FindRepoRoot();
        var adx = File.ReadAllText(Path.Combine(root, "hunts", "beacon-periodicity.kql"));
        var sentinel = File.ReadAllText(Path.Combine(root, "sentinel", "analytics-rule.kql"));
        foreach (var text in new[] { adx, sentinel })
        {
            Assert.Contains("Calls >= 8", text);
            Assert.Contains("StdevGap / AvgGap < 0.25", text);
        }
        Assert.Contains("BeaconSightings_CL", sentinel);
        Assert.DoesNotContain("BeaconSightings_CL", adx);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "hunts", "beacon-periodicity.kql")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new FileNotFoundException("hunts/beacon-periodicity.kql");
    }
}
