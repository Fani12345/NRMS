using System.Text;
using NRMS.Application.Contracts;
using NRMS.Application.Services;
using NRMS.Domain.Enums;
using NRMS.Infrastructure;
using NRMS.Infrastructure.Persistence.Sqlite;
using NRMS.Infrastructure.Storage;
using Xunit;

namespace NRMS.Tests;

public sealed class NrmsCaseServiceSqliteTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dbPath;
    private readonly string _evidenceDir;

    public NrmsCaseServiceSqliteTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "NRMS_Sqlite_Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _dbPath = Path.Combine(_tempDir, "nrms_test.db");
        _evidenceDir = Path.Combine(_tempDir, "evidence");
        Directory.CreateDirectory(_evidenceDir);
    }

    [Fact]
    public async Task EndToEnd_WithSqlite_PersistsCaseEvidenceAndAuditEvents()
    {
        var clock = new FixedClock(new DateTime(2025, 12, 17, 0, 0, 0, DateTimeKind.Utc));
        var refGen = new SimpleCaseReferenceGenerator();
        var hash = new Sha256HashService();

        var db = new SqliteDb(_dbPath);
        db.EnsureCreated();

        var caseRepo = new SqliteCaseRepository(db);
        var auditRepo = new SqliteAuditEventRepository(db);
        var storage = new FileSystemEvidenceStorage(_evidenceDir);

        var svc = new NrmsCaseService(caseRepo, auditRepo, clock, refGen, hash, storage);

        var create = await svc.CreateCaseAsync(new CreateCaseCommand(CaseType.Allocation, "fani"));

        var bytes = Encoding.UTF8.GetBytes("sqlite persistence test");
        await using var ms = new MemoryStream(bytes);

        var attach = await svc.AttachEvidenceAsync(new AttachEvidenceCommand(
            CaseId: create.CaseId,
            ImportedBy: "fani",
            FileName: "proof.pdf",
            SourceDescription: "demo",
            Content: ms
        ));

        var loaded = await caseRepo.GetAsync(create.CaseId);
        Assert.NotNull(loaded);
        Assert.Equal(create.CaseReference, loaded!.CaseReference);
        Assert.Single(loaded.Evidence);

        Assert.True(File.Exists(Path.Combine(_evidenceDir, attach.StoredPath)));

        var events = await auditRepo.GetByCaseAsync(create.CaseId);
        Assert.Contains(events, e => e.EventType == AuditEventType.CaseCreated);
        Assert.Contains(events, e => e.EventType == AuditEventType.EvidenceAttached);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, recursive: true);
        }
        catch
        {
        }
    }

    private sealed class FixedClock : NRMS.Application.Abstractions.IClock
    {
        public FixedClock(DateTime utcNow) =>
            UtcNow = utcNow.Kind == DateTimeKind.Utc ? utcNow : DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        public DateTime UtcNow { get; }
    }
}
