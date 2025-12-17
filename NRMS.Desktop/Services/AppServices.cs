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

    // Expose repositories for read-only UI operations
    public ICaseRepository CaseRepository { get; }

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
        var refGen = new SimpleCaseReferenceGenerator();
        var hash = new Sha256HashService();
        var storage = new FileSystemEvidenceStorage(evidenceDir);

        CaseRepository = caseRepo;
        CaseService = new NrmsCaseService(caseRepo, auditRepo, clock, refGen, hash, storage);
    }
}
