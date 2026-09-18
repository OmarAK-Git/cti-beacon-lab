# BeaconLab

Teaching map for the CTI beacon lab. Every fixture below names the Azure service it stands in for. Pipeline order.

## Producer

**Stands in for** a sensor or forwarder publishing to Event Hubs.
**Same** hub name `raw-events`, JSON body, partition key `DeviceName`.
**Only at swap-in** the connection string becomes a namespace plus managed identity.
**Not this** a Function. It is the publisher.

## Event Hubs emulator, hub `raw-events`

**Stands in for** an Event Hub in a namespace.
**Same** hub name, partitions, send and receive.
**Only at swap-in** Entra ID, Capture, geo-replication, retention that survives a restart. Emulator data dies when the container stops.
**Not this** Azurite. Azurite is not the hub.

## Azurite

**Stands in for** the Storage account behind `AzureWebJobsStorage`.
**Same** the Functions host checkpoint blobs and the `failed-events` failure store.
**Only at swap-in** a real storage account.
**Not this** Event Hubs. Checkpoints are how the Function remembers its offset.

## Normalize

**Stands in for** nothing. This is the real Function.
**Same** consumer group `fn`, hubs `raw-events` and `normalized-events`.
**Only at swap-in** the identity connection from `config/azure.settings.json`.
**Not this** a stand-in mapping or grader step.

## Hub `normalized-events` and consumer group `adx`

**Stands in for** the Event Hub that ADX will read.
**Same** hub name, group name `adx`.
**Only at swap-in** an actual ADX data connection on `adx`.
**Not this** the function's group `fn`. Consumer group `test-reader` exists only so Task 10 can see the output without joining `adx`.

## BeaconLab.Mapping

**Stands in for** the ADX Event Hubs data connection plus the JSON ingestion mapping.
**Same** the `Path` list for every `BeaconEvent` property.
**Only at swap-in** the cluster, table, ingestion mapping, and connection on `normalized-events` group `adx`.
**Not this** a hub reader. Mapping does not read `adx`.

## Grader

**Stands in for** you, running the hunt and checking it.
**Same** the periodicity rule thresholds against mapped rows.
**Only at swap-in** you run the same hunt in ADX or Sentinel.
**Not this** ADX or Sentinel. The grader may see the campaign. The KQL may not.

## sentinel/rows

**Stands in for** the Logs Ingestion API body destined for `BeaconSightings_CL`.
**Same** every beacon field including `TimeGenerated`.
**Only at swap-in** a data collection endpoint, rule, and custom table.
**Not this** an already-ingested log.

## CommentIncident

**Stands in for** nothing. This is the real HTTP function the playbook will call.
**Same** comment text naming host and domain.
**Only at swap-in** the Logic App posts that comment to the incident.
**Not this** the playbook itself.

## sentinel/automation-rule.json

**Stands in for** the automation rule record (not an ARM export).
**Same** trigger on incident created, condition on analytics rule `Beacon periodicity`, action run playbook.
**Only at swap-in** create it from Microsoft Sentinel Configuration Automation in the Defender portal.
**Not this** conditioning on incident title.

## sentinel/playbook.md

**Stands in for** the Logic App playbook you build in the portal.
**Same** the runbook steps: Microsoft Sentinel incident trigger, CommentIncident, Add comment, Automation Contributor role.
**Only at swap-in** the portal build.
**Not this** a workflow export. The playbook file is a runbook, not a workflow export.

## Local config

- `config/local.settings.json` — emulator connection with documented key `SAS_KEY_VALUE`, Azurite via `UseDevelopmentStorage=true`.
- `config/azure.settings.json` — unread until swap-in; managed identity placeholders; no keys.
- `emulator/` — Learn article compose + Config. Run compose from `emulator/` so `CONFIG_PATH=./Config.json` resolves. Do not start here unless running the bad-body path.
