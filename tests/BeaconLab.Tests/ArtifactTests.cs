using System.IO;

namespace BeaconLab.Tests;

public class ArtifactTests
{
    [Fact]
    public void Artifacts_name_the_real_services()
    {
        var root = FindRepoRoot();

        var azure = File.ReadAllText(Path.Combine(root, "config", "azure.settings.json"));
        Assert.Contains("EventHubConnection__fullyQualifiedNamespace", azure);
        Assert.Contains("managedidentity", azure);
        Assert.DoesNotContain("SharedAccessKey", azure);

        var automation = File.ReadAllText(Path.Combine(root, "sentinel", "automation-rule.json"));
        Assert.Contains("incident created", automation);
        Assert.Contains("Beacon periodicity", automation);
        Assert.DoesNotContain("incidentTitle", automation);

        var playbook = File.ReadAllText(Path.Combine(root, "sentinel", "playbook.md"));
        Assert.Contains("Microsoft Sentinel incident", playbook);
        Assert.Contains("CommentIncident", playbook);
        Assert.Contains("Microsoft Sentinel Automation Contributor", playbook);

        var readme = File.ReadAllText(Path.Combine(root, "README.md"));
        Assert.Contains("raw-events", readme);
        Assert.Contains("Azurite", readme);
        Assert.Contains("fn", readme);
        Assert.Contains("adx", readme);
        Assert.Contains("Azurite is not the hub", readme);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "hunts", "beacon-periodicity.kql")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new FileNotFoundException("hunts/beacon-periodicity.kql");
    }
}
