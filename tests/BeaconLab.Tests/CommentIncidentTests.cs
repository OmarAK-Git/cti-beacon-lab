using BeaconLab.Sentinel;

public class CommentIncidentTests
{
    [Fact]
    public void CommentIncident_returns_comment()
    {
        var result = IncidentCommenter.Create(
            "/subscriptions/lab/resourceGroups/lab/providers/Microsoft.OperationalInsights/workspaces/lab/providers/Microsoft.SecurityInsights/incidents/1",
            "WKSTN-04",
            "cdn-updates.example");
        Assert.Equal(200, result.Status);
        Assert.Contains("WKSTN-04", result.Body);
        Assert.Contains("cdn-updates.example", result.Body);
        Assert.Contains("periodicity hunt", result.Body);
    }

    [Fact]
    public void CommentIncident_rejects_missing_host()
    {
        var result = IncidentCommenter.Create("/subscriptions/lab/incidents/1", null, "cdn-updates.example");
        Assert.Equal(400, result.Status);
        Assert.Null(result.Body);
    }
}
