public class NormalizeBindingTests
{
    [Fact]
    public void Normalize_on_fn_not_default()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "src", "BeaconLab.Functions", "Normalize.cs")))
            root = root.Parent;
        var source = File.ReadAllText(Path.Combine(root!.FullName, "src", "BeaconLab.Functions", "Normalize.cs"));
        Assert.Contains("ConsumerGroup = \"fn\"", source);
        Assert.Contains("normalized-events", source);
        Assert.DoesNotContain("ConsumerGroup = \"$Default\"", source);
    }
}
