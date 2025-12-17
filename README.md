\# NRMS — Numbering Resource Management System (Demo)



Offline-first desktop demo of a regulator-grade Numbering Resource Management System (NRMS) focused on:

\- Allocation cases with evidence indexing and Secure Hash Algorithm 256-bit (SHA-256) checksums

\- Separation of Duties (SoD) between Review and Approval

\- Append-only audit trail events

\- Controlled outputs (Portable Document Format (PDF), Comma-Separated Values (CSV), and ZIP export packs)



\## Solution structure

\- NRMS.Domain — core domain model and rules

\- NRMS.Application — use cases (commands/queries), validation, orchestration

\- NRMS.Infrastructure — persistence, file storage, export generation (later)

\- NRMS.Desktop — Windows Presentation Foundation (WPF) user interface

\- NRMS.Tests — xUnit tests (Test-Driven Development (TDD))



\## Build and test

```bash

dotnet test



