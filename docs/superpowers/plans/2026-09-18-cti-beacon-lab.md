# CTI Beacon Lab Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the fixture-first CTI beacon lab from the spec, so the periodicity hunt, the Function, and the Sentinel artifacts exist and the tests in the spec pass without an Azure subscription.

**Architecture:** Class libraries hold the campaign expander, the ADX mapping stand-in, the grader, and the incident commenter. A console app publishes. One isolated-worker Function app hosts `Normalize` and `CommentIncident`. KQL and Sentinel files are checked in and asserted as text. The Event Hubs emulator is only required for the bad-body test.

**Tech Stack:** C# `net8.0`, xUnit, Azure Functions isolated worker, Azure.Messaging.EventHubs, Azure.Storage.Blobs, Event Hubs emulator plus Azurite via the Microsoft compose file.

**Spec:** `docs/superpowers/specs/2026-09-18-cti-beacon-lab-design.md`

## Global Constraints

- C# only. Function host is isolated worker. `FUNCTIONS_WORKER_RUNTIME=dotnet-isolated`. No in-process, no Python.
- TFM is `net8.0` on every project.
- Hubs are `raw-events` and `normalized-events`. Function consumer group is `fn`. Group `adx` is created and never read by local code. `$Default` is never the function's group.
- Beacon interval, jitter, and any `isBeacon` flag never appear on `BeaconEvent`.
- Hunt thresholds are exactly `8` gaps and coefficient of variation strictly less than `0.25`. C# uses sample standard deviation (divide by `n - 1`) because KQL `stdev` is the sample statistic.
- Campaign `seed` is `1`. Jitter is `Random(seed).Next(-9, 10)` seconds, because 15 percent of 60 seconds is 9. Beacon base time is `2026-09-18T08:00:00Z`.
- Mapping does not consume `adx`. It maps events the function already built.
- No Azure subscription, no real keys. Emulator shared access key is the documented literal `SAS_KEY_VALUE`.
- Only `Normalize_bad_body_writes_failure_blob_and_continues` may require Docker. If port `5672` is closed it fails with the exact message `emulator not running`.
- Teaching labels (stands in for, same, only at swap-in, not this) live in `README.md` and in the header comment of each stand-in.

## Task Summary

1. Contracts and campaign expander: `BeaconEvent`, campaign records, deterministic fixture expansion, validation tests.
2. Producer: device-name partition key and recording publisher.
3. ADX mapping stand-in: complete `Path` list and recorded mapping failures.
4. Periodicity grader: group by device/domain, calculate inter-arrival gaps, require at least 8 gaps and CV below 0.25.
5. KQL files: ADX `BeaconSightings` and Sentinel `BeaconSightings_CL` queries with identical thresholds.
6. Sentinel row: every beacon field including `TimeGenerated`.
7. Incident commenter: HTTP-shaped result with 400 for missing fields and a 200 comment for valid host/domain.
8. Isolated Functions app: `Normalize` uses Event Hubs group `fn`, outputs to `normalized-events`, writes bad bodies to `failed-events`; `CommentIncident` is the HTTP function.
9. Config and artifacts: local emulator settings, managed-identity swap-in settings, Sentinel rule/entity/automation/playbook files, emulator compose/config, and README.
10. Bad-body integration test: emulator and Azurite are required only for this path; the test must distinguish `emulator not running` from `function host not running`.
11. Full unit suite: run all tests other than the Docker-dependent path without an Azure subscription.

## Execution Handoff

Plan saved to `docs/superpowers/plans/2026-09-18-cti-beacon-lab.md`. Two execution options:

1. Subagent-Driven (recommended). A fresh subagent per task, review between tasks.
2. Inline Execution. Tasks in this session, with checkpoints.

Which approach?
