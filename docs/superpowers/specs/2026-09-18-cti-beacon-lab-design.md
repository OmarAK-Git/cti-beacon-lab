# CTI Beacon Lab

**Date:** 2026-09-18
**Status:** Design, approved in chat. Implementation plan not started.

## Purpose

This lab exists so you can get sharp on four Azure services before a work engagement: Azure Event Hubs, Azure Functions (isolated worker), Azure Data Explorer, and Microsoft Sentinel automation. You learn by building one detection, then by seeing exactly which local piece stands in for which cloud service.

The detection is a cyber threat intelligence hunt for host beaconing. A campaign file knows which host is calling back, and on what interval. The events that travel the pipeline do not. The hunt has to measure the cadence.

There is no Azure subscription yet. Everything that can run on the Event Hubs emulator runs now. Every resource that cannot is a checked-in artifact with a named swap-in, so the day a subscription appears you change settings instead of learning the shape under pressure.

## Locked decisions

- One spec for the whole pipeline, built in production order. Not four projects.
- C# only. .NET isolated worker (`FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`) for the Function app. The producer is a console app in the same solution.
- One Function app, two functions: `Normalize` and `CommentIncident`.
- One hunt: periodic beacons. No second technique, threat-intel feed, or MISP.
- Fixture-first. The Kusto emulator is not used. It has no Event Hubs connection, and using it would teach a path that does not exist.
- Python is not used. Staying in C# lets the playbook use the Azure Functions action for real.
- The hunt never reads the campaign interval. Only the grader does.

## How to read a fixture

Every local stand-in carries the same four labels:

1. **Stands in for.** The Azure service, by its real name.
2. **Same.** What does not change at swap-in.
3. **Only at swap-in.** What this fixture cannot teach you.
4. **Not this.** The nearby service people confuse it with.

A C# test is never called ADX. A blob is never called a dead-letter queue. Azurite is never called the hub.

## Pipeline

Campaign file -> Producer -> `raw-events` -> `Normalize` -> `normalized-events` -> mapping fixture -> row files -> grader. In parallel, the ADX hunt, Sentinel analytics rule, automation rule, playbook runbook, and `CommentIncident` preserve the production order as checked-in artifacts.

Runs today: campaign, producer, both hubs on the emulator, `Normalize`, the mapping fixture, row files, the grader, and `CommentIncident` invoked by a test. The ADX query, analytics rule, automation rule, and playbook runbook remain files until a subscription exists.

`normalized-events` has no local reader. Consumer group `adx` is created and idle until a cluster exists. Mapping takes events `Normalize` already built; it does not consume that hub.

## Event and campaign

`BeaconEvent` contains `TimeGenerated`, `DeviceName`, `AccountName`, `ProcessName`, `CommandLine`, `Sha256`, `DestinationDomain`, `DestinationIp`, `DestinationPort`, and `BytesOut`. `TimeGenerated` is required. Interval, jitter, and an `isBeacon` ground-truth flag are forbidden on the event object.

The lab sample uses host `WKSTN-04`, account `jdoe`, process `rundll32.exe`, domain `cdn-updates.example`, documentation IP `203.0.113.44`, port 443, 428 bytes, seed 1, ten sightings, 60-second interval, and 15 percent jitter. Noise hosts are `WKSTN-11` with alternating 5/600-second gaps and `WKSTN-12` with only three sightings.

## The hunt

Group by `DeviceName` and `DestinationDomain`, sort by time, and calculate inter-arrival gaps. Flag a pair only when there are at least `8` gaps, average gap is greater than zero, and sample coefficient of variation (`stdev / avg`) is strictly less than `0.25`.

ADX uses table `BeaconSightings`; Sentinel uses `BeaconSightings_CL`. Both KQL files contain `Calls >= 8` and `StdevGap / AvgGap < 0.25`. Analytics rules query the Sentinel table, never ADX.

## Components and swap-in labels

- **Producer:** stands in for a sensor or forwarder publishing to Event Hubs. Same hub name `raw-events`, JSON body, and `DeviceName` partition key. Only at swap-in does the connection become a namespace plus managed identity. Not this a Function.
- **Event Hubs emulator:** stands in for Event Hubs. Same hub names, partitions, send, and receive. Only at swap-in come Entra ID, Capture, geo-replication, and durable retention. Not this Azurite.
- **Azurite:** stands in for the Storage account behind `AzureWebJobsStorage`, checkpoint blobs, and the `failed-events` failure store. Only at swap-in is it replaced by a real storage account. Not this Event Hubs; checkpoints are the Function cursor.
- **Normalize:** stands in for nothing; it is the real isolated-worker Function. Same `fn` group and both hub names. Only at swap-in does the identity connection change. Not this a mapping stand-in.
- **normalized-events / `adx`:** stands in for the hub ADX will read. Same hub and group name. Only at swap-in is an ADX data connection created. Not this the Function's `fn` group.
- **Mapping:** stands in for the ADX Event Hubs data connection and JSON ingestion mapping. Same `Path` list for each event property. Only at swap-in are the cluster, table, mapping, connection, and identity supplied. Not this a hub reader.
- **Grader:** stands in for a person checking the hunt. Same thresholds against mapped rows. Only at swap-in is the hunt run in ADX or Sentinel. Not this ADX or Sentinel.
- **Sentinel rows:** stand in for the Logs Ingestion API body destined for `BeaconSightings_CL`. Same every field including `TimeGenerated`. Only at swap-in are DCE, DCR, custom table, and publisher role supplied. Not this an already-ingested log.
- **CommentIncident:** stands in for nothing; it is the real HTTP Function the playbook calls. Same comment naming host and domain. Only at swap-in does Logic Apps post that comment to an incident. Not this the playbook.
- **Automation rule:** stands in for the Sentinel automation rule record, not an ARM export. Same trigger `incident created`, condition `Beacon periodicity`, and run-playbook action. Only at swap-in is it created in the Defender portal. Not this conditioning on incident title.
- **Playbook:** stands in for the Logic App playbook built in the portal. Same Sentinel incident trigger, `CommentIncident`, Add comment action, and Microsoft Sentinel Automation Contributor role. Only at swap-in is it built in the portal. Not this a workflow export.

## Error handling

Event Hubs has no dead-letter queue; that is Service Bus. A malformed or non-`BeaconEvent` body is written with its reason to the `failed-events` blob container and the invocation succeeds so the checkpoint advances. Transient hub or checkpoint failures are not swallowed. A missing mapping path is recorded as a mapping failure and the row is not passed to the grader. `CommentIncident` returns 400 when incident ARM id, host, or domain is missing and 200 with comment text otherwise.

## Configuration and tests

`config/local.settings.json` uses the emulator connection and `UseDevelopmentStorage=true`. `config/azure.settings.json` is unread until swap-in and contains managed-identity placeholders but no keys. The eleven non-Docker tests cover contracts, validation, partition key, Function binding, mapping, grader, KQL, Sentinel row, and comment behavior. The twelfth test requires Docker and must fail with `emulator not running` when port 5672 is closed.

## Swap-in order

Create Event Hubs with `raw-events`, `normalized-events`, `fn`, and `adx`; configure checkpoint storage; create the ADX table/mapping/connection; create the Logs Ingestion endpoint/rule/table; create the scheduled Sentinel rule with the KQL and entity map; build the playbook; create the automation rule; run the producer and wait for the scheduled rule window.

## Out of scope

No second hunt, enrichment, watchlists, TI indicators, notebooks, Sentinel data lake, whole-estate Bicep, renamed playbooks, or ADX queries from Sentinel analytics rules. The lab is done when the unit suite passes, every folder's Azure stand-in is clear, the config has no secrets, both KQL files match the grader, and the playbook file is a runbook rather than a generated workflow.

Implementation is a later plan. This document is the spec.
