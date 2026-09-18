namespace BeaconLab.Sentinel;

// This is the real comment the playbook will request. It does not call Sentinel.
// A test calling Create stands in for the Logic App. The function is not the playbook.
public static class IncidentCommenter
{
    public static CommentResult Create(string? incidentArmId, string? host, string? domain)
    {
        if (string.IsNullOrWhiteSpace(incidentArmId) || string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(domain))
            return new CommentResult(400, null);
        return new CommentResult(200, $"periodicity hunt fired for host {host} calling {domain}.");
    }
}

public sealed record CommentResult(int Status, string? Body);
