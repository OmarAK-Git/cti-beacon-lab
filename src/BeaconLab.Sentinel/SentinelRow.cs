using System.Text.Json;
using BeaconLab.Contracts;

namespace BeaconLab.Sentinel;

// Stands in for: the Logs Ingestion API body, table BeaconSightings_CL.
// Same: fields, including TimeGenerated. Only at swap-in: DCE, DCR, Monitoring Metrics Publisher.
// Not this: an ingested log.
public static class SentinelRow
{
    public static string ToJson(BeaconEvent row) => JsonSerializer.Serialize(row);
}
