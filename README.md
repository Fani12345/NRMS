# NRMS — Numbering Resource Management System (Demo)

Offline-first desktop demonstration of a regulator-grade **Numbering Resource Management System (NRMS)**.  
This prototype focuses on **traceability, data integrity, and auditability** for case-based regulatory workflows.

## What this demo currently supports

- **Case creation** (e.g., Allocation)
- **Open an existing case** by **Case Reference**
- **Evidence attachment** (file stored + **Secure Hash Algorithm 256-bit (SHA-256)** checksum recorded)
- **SQLite persistence** (cases, evidence, and audit events persist across restarts)
- **Append-only audit trail** (e.g., `CaseCreated`, `EvidenceAttached`)

Planned (future slices):
- Separation of Duties (SoD) between Review and Approval
- Controlled outputs (Portable Document Format (PDF), Comma-Separated Values (CSV), ZIP export packs)
- Number range management, allocations, reservations, reporting

## Why this matters for a regulator

This is a compact demonstration of:
- **Chain-of-custody** for evidence (stored artifact + hash)
- **Non-repudiation support** (audit events recorded for each action)
- **Reproducibility** (same case can be re-opened and verified by reference)

## Solution structure

- **NRMS.Domain** — core domain model and rules
- **NRMS.Application** — use cases (commands/queries), validation, orchestration
- **NRMS.Infrastructure** — SQLite persistence, file storage, hashing, system clock
- **NRMS.Desktop** — Windows Presentation Foundation (WPF) user interface
- **NRMS.Tests** — xUnit tests (Test-Driven Development (TDD))

## Demo workflow (3–5 minutes)

1. **Create Case**
   - Enter **Created By**
   - Select **Case Type**
   - Click **Create Case**
   - Note the **Case Reference** and **Case ID**

2. **Attach Evidence**
   - Enter **Source**
   - Click **Pick File...**
   - Click **Attach**
   - Verify:
     - Evidence grid shows **SHA-256**
     - Audit trail shows **EvidenceAttached**

3. **Open Case**
   - Copy a previous **Case Reference**
   - Paste into **Open Case**
   - Click **Open Case**
   - Verify evidence + audit trail reload

## Build and test

From the repository root:

```bash
dotnet build
dotnet test
