using System.Net;
using System.Text.Json;
using BeaconLab.Sentinel;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace BeaconLab.Functions;

// This is the real HTTP function the playbook will call. It does not call Sentinel.
public sealed class CommentIncident
{
    [Function(nameof(CommentIncident))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        CommentRequest? payload = null;
        try
        {
            payload = await JsonSerializer.DeserializeAsync<CommentRequest>(
                req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException)
        {
            // treat as missing fields
        }

        var result = IncidentCommenter.Create(payload?.IncidentArmId, payload?.Host, payload?.Domain);
        var response = req.CreateResponse((HttpStatusCode)result.Status);
        if (result.Body is not null)
            await response.WriteStringAsync(result.Body);
        return response;
    }

    private sealed record CommentRequest(string? IncidentArmId, string? Host, string? Domain);
}
