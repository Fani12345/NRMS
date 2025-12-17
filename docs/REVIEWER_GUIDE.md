# NRMS Prototype — Reviewer Guide

## What this demo shows
A small, working prototype demonstrating regulator-style traceability:
1) Create a case
2) Attach evidence (stored file + SHA-256)
3) Persist to SQLite
4) View audit trail (CaseCreated, EvidenceAttached)
5) Re-open case by Case Reference

## Quick verification (3–5 minutes)
- Create a case → confirm CaseReference and CaseId populate
- Attach a file → confirm Evidence row shows SHA-256 and Stored Path
- Audit trail shows CaseCreated and EvidenceAttached
- Restart app → create another case (no duplicate reference error)
- Open an existing case by Case Reference → evidence and audit reload

## How to run
```bash
dotnet build
dotnet test
dotnet run --project NRMS.Desktop
