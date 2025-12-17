using System.Text;
using NRMS.Application.Contracts;
using NRMS.Application.Services;
using NRMS.Domain.Enums;
using NRMS.Infrastructure;
using NRMS.Infrastructure.Storage;
using Xunit;

namespace NRMS.Tests;

public sealed class NrmsCaseServiceTests : IDisposable
{
    private readonly string _tempEvidenceDir;

    public NrmsCaseServiceTests()
    {
        _tempEvidenceDir = Path.Combine(Path.GetTempPath(), "NRMS_Tests_Evidence", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempEvidenceDir);
    }

    [Fact]
    public async Task CreateCase_CreatesCase_AndWritesAuditEvent()
    {
        var caseRepo = new InMemoryCaseRepository();
        var auditRepo = new InMemoryAuditEventRepository();

        var clock = new FixedClock(new DateTime(2025, 12, 17, 0, 0, 0, DateTimeKind.Utc));
        var refGen = new SimpleCaseReferenceGenerator();
        var hash = new Sha256HashService();
        var storage = new FileSystemEvidenceStorage(_tempEvidenceDir);

        var svc = new NrmsCaseService(caseRepo, auditRepo, clock, refGen, hash, storage);

        var result = await svc.CreateCaseAsync(new CreateCaseCommand(CaseType.Allocation, "fani"));

        Assert.NotEqual(Guid.Empty, result.CaseId);
        Assert.StartsWith("CASE-2025-", result.CaseReference);

        var created = await caseRepo.GetAsync(result.CaseId);
        Assert.NotNull(created);
        Assert.Equal(result.CaseReference, created!.CaseReference);
        Assert.Equal(CaseType.Allocation, created.CaseType);

        var events = await auditRepo.GetByCaseAsync(result.CaseId);
        Assert.Contains(events, e => e.EventType == AuditEventType.CaseCreated);
    }

    [Fact]
    public async Task AttachEvidence_ComputesSha256_SavesFile_AddsAuditEvent()
    {
        var caseRepo = new InMemoryCaseRepository();
        var auditRepo = new InMemoryAuditEventRepository();

        var clock = new FixedClock(new DateTime(2025, 12, 17, 0, 0, 0, DateTimeKind.Utc));
        var refGen = new SimpleCaseReferenceGenerator();
        var hash = new Sha256HashService();
        var storage = new FileSystemEvidenceStorage(_tempEvidenceDir);

        var svc = new NrmsCaseService(caseRepo, auditRepo, clock, refGen, hash, storage);

        var create = await svc.CreateCaseAsync(new CreateCaseCommand(CaseType.Allocation, "fani"));

        var bytes = Encoding.UTF8.GetBytes("NRMS evidence test content");
        await using var ms = new MemoryStream(bytes);

        var attach = await svc.AttachEvidenceAsync(new AttachEvidenceCommand(
            CaseId: create.CaseId,
            ImportedBy: "fani",
            FileName: "test.pdf",
            SourceDescription: "demo",
            Content: ms
        ));

        Assert.Equal(64, attach.Sha256.Length);
        Assert.True(attach.Sha256.All(c => "0123456789abcdef".Contains(c)));

        var storedFullPath = Path.Combine(_tempEvidenceDir, attach.StoredPath);
        Assert.True(File.Exists(storedFullPath));

        var c = await caseRepo.GetAsync(create.CaseId);
        Assert.NotNull(c);
        Assert.Single(c!.Evidence);

        var events = await auditRepo.GetByCaseAsync(create.CaseId);
        Assert.Contains(events, e => e.EventType == AuditEventType.EvidenceAttached);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempEvidenceDir))
                Directory.Delete(_tempEvidenceDir, recursive: true);
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
