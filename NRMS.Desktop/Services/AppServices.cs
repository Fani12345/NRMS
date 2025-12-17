using NRMS.Application.Abstractions;
using NRMS.Application.Services;
using NRMS.Infrastructure;
using NRMS.Infrastructure.Persistence.Sqlite;
using NRMS.Infrastructure.Storage;
using System.IO;

namespace NRMS.Desktop.Services;

public sealed class AppServices
{
    public NrmsCaseService CaseService { get; }
    public ICaseRepository CaseRepository { get; }
    public IAuditEventRepository AuditRepository { get; }

    public AppServices()
    {
        var baseDir = AppContext.BaseDirectory;

        var dataDir = Path.Combine(baseDir, "data");
        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "nrms.db");
        var evidenceDir = Path.Combine(dataDir, "evidence");
        Directory.CreateDirectory(evidenceDir);

        var db = new SqliteDb(dbPath);
        db.EnsureCreated();

        var caseRepo = new SqliteCaseRepository(db);
        var auditRepo = new SqliteAuditEventRepository(db);

        var clock = new SystemClock();

        // Seed the case reference generator from existing DB rows for the current year
        var lastSeq = GetLastUsedCaseSequenceForYear(db, clock.UtcNow.Year);
        var refGen = new SimpleCaseReferenceGenerator(lastSeq);

        var hash = new Sha256HashService();
        var storage = new FileSystemEvidenceStorage(evidenceDir);

        CaseRepository = caseRepo;
        AuditRepository = auditRepo;
        CaseService = new NrmsCaseService(caseRepo, auditRepo, clock, refGen, hash, storage);
    }

    private static int GetLastUsedCaseSequenceForYear(SqliteDb db, int year)
    {
        // We assume CaseReference format: CASE-YYYY-NNNN
        var prefix = $"CASE-{year}-";

        using var conn = db.OpenConnection();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
SELECT CaseReference
FROM Cases
WHERE CaseReference LIKE @prefix || '%'
ORDER BY CaseReference DESC
LIMIT 1;
";
        cmd.Parameters.AddWithValue("@prefix", prefix);

        var scalar = cmd.ExecuteScalar();
        if (scalar is null || scalar is DBNull)
            return 0;

        var caseRef = (string)scalar;

        // Expected: CASE-2025-0008
        var parts = caseRef.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return 0;

        return int.TryParse(parts[2], out var n) ? n : 0;
    }
}
