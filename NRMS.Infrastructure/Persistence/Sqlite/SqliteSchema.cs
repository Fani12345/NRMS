namespace NRMS.Infrastructure.Persistence.Sqlite;

internal static class SqliteSchema
{
    public const int SchemaVersion = 1;

    public const string CreateSchemaSql = @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS __schema_info (
    SchemaVersion INTEGER NOT NULL
);

INSERT INTO __schema_info (SchemaVersion)
SELECT 0
WHERE NOT EXISTS (SELECT 1 FROM __schema_info);

CREATE TABLE IF NOT EXISTS Cases (
    CaseId TEXT NOT NULL PRIMARY KEY,
    CaseReference TEXT NOT NULL,
    CaseType INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    CreatedBy TEXT NOT NULL,
    CreatedAtUtc TEXT NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_Cases_CaseReference ON Cases (CaseReference);

CREATE TABLE IF NOT EXISTS EvidenceItems (
    EvidenceId TEXT NOT NULL PRIMARY KEY,
    CaseId TEXT NOT NULL,
    FileName TEXT NOT NULL,
    SourceDescription TEXT NOT NULL,
    ImportedAtUtc TEXT NOT NULL,
    ImportedBy TEXT NOT NULL,
    Sha256 TEXT NOT NULL,
    StoredPath TEXT NOT NULL,
    FOREIGN KEY (CaseId) REFERENCES Cases (CaseId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_EvidenceItems_CaseId ON EvidenceItems (CaseId);

CREATE TABLE IF NOT EXISTS AuditEvents (
    AuditEventId TEXT NOT NULL PRIMARY KEY,
    CaseId TEXT NOT NULL,
    EventType INTEGER NOT NULL,
    OccurredAtUtc TEXT NOT NULL,
    Actor TEXT NOT NULL,
    ObjectType TEXT NOT NULL,
    ObjectId TEXT NOT NULL,
    DetailsJson TEXT NOT NULL,
    FOREIGN KEY (CaseId) REFERENCES Cases (CaseId) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_AuditEvents_CaseId ON AuditEvents (CaseId);
";
}
