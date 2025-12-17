using System.Text.Json;
using NRMS.Application.Abstractions;
using NRMS.Application.Contracts;
using NRMS.Domain.Entities;
using NRMS.Domain.Enums;
using NRMS.Domain.Exceptions;
using NRMS.Domain.ValueObjects;

namespace NRMS.Application.Services;

public sealed class NrmsCaseService
{
    private readonly ICaseRepository _caseRepository;
    private readonly IAuditEventRepository _auditRepository;
    private readonly IClock _clock;
    private readonly ICaseReferenceGenerator _caseRefGen;
    private readonly IHashService _hashService;
    private readonly IEvidenceStorage _evidenceStorage;

    public NrmsCaseService(
        ICaseRepository caseRepository,
        IAuditEventRepository auditRepository,
        IClock clock,
        ICaseReferenceGenerator caseRefGen,
        IHashService hashService,
        IEvidenceStorage evidenceStorage)
    {
        _caseRepository = caseRepository;
        _auditRepository = auditRepository;
        _clock = clock;
        _caseRefGen = caseRefGen;
        _hashService = hashService;
        _evidenceStorage = evidenceStorage;
    }

    public async Task<CreateCaseResult> CreateCaseAsync(CreateCaseCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.CreatedBy))
            throw new DomainException("CreatedBy is required.");

        var now = EnsureUtc(_clock.UtcNow);
        var caseId = Guid.NewGuid();
        var caseRef = _caseRefGen.Next(cmd.CaseType, now);

        var nrmsCase = new NrmsCase(caseId, caseRef, cmd.CaseType, cmd.CreatedBy.Trim(), now);

        await _caseRepository.AddAsync(nrmsCase, ct).ConfigureAwait(false);

        var details = JsonSerializer.Serialize(new
        {
            nrmsCase.CaseReference,
            CaseType = nrmsCase.CaseType.ToString(),
            nrmsCase.CreatedBy
        });

        var auditEvent = new AuditEvent(
            auditEventId: Guid.NewGuid(),
            eventType: AuditEventType.CaseCreated,
            occurredAtUtc: now,
            actor: nrmsCase.CreatedBy,
            caseId: nrmsCase.CaseId,
            objectType: "Case",
            objectId: nrmsCase.CaseId.ToString(),
            detailsJson: details
        );

        await _auditRepository.AppendAsync(auditEvent, ct).ConfigureAwait(false);

        return new CreateCaseResult(caseId, caseRef);
    }

    public async Task<AttachEvidenceResult> AttachEvidenceAsync(AttachEvidenceCommand cmd, CancellationToken ct = default)
    {
        if (cmd.CaseId == Guid.Empty) throw new DomainException("CaseId is required.");
        if (string.IsNullOrWhiteSpace(cmd.ImportedBy)) throw new DomainException("ImportedBy is required.");
        if (string.IsNullOrWhiteSpace(cmd.FileName)) throw new DomainException("FileName is required.");
        if (cmd.Content is null) throw new DomainException("Content is required.");

        var nrmsCase = await _caseRepository.GetAsync(cmd.CaseId, ct).ConfigureAwait(false);
        if (nrmsCase is null) throw new DomainException("Case not found.");

        var now = EnsureUtc(_clock.UtcNow);

        using var buffer = new MemoryStream();
        await cmd.Content.CopyToAsync(buffer, ct).ConfigureAwait(false);
        buffer.Position = 0;

        var sha = await _hashService.ComputeSha256Async(buffer, ct).ConfigureAwait(false);
        var shaVo = new Sha256(sha);

        buffer.Position = 0;
        var evidenceId = Guid.NewGuid();
        var storedPath = await _evidenceStorage.SaveAsync(evidenceId, cmd.FileName.Trim(), buffer, ct).ConfigureAwait(false);

        var evidence = new EvidenceItem(
            evidenceId: evidenceId,
            fileName: cmd.FileName.Trim(),
            sourceDescription: cmd.SourceDescription ?? string.Empty,
            importedAtUtc: now,
            importedBy: cmd.ImportedBy.Trim(),
            sha256: shaVo,
            storedPath: storedPath
        );

        nrmsCase.AddEvidence(evidence);

        await _caseRepository.UpdateAsync(nrmsCase, ct).ConfigureAwait(false);

        var details = JsonSerializer.Serialize(new
        {
            evidence.EvidenceId,
            evidence.FileName,
            evidence.StoredPath,
            Sha256 = evidence.Sha256.Value
        });

        var auditEvent = new AuditEvent(
            auditEventId: Guid.NewGuid(),
            eventType: AuditEventType.EvidenceAttached,
            occurredAtUtc: now,
            actor: cmd.ImportedBy.Trim(),
            caseId: nrmsCase.CaseId,
            objectType: "Evidence",
            objectId: evidence.EvidenceId.ToString(),
            detailsJson: details
        );

        await _auditRepository.AppendAsync(auditEvent, ct).ConfigureAwait(false);

        return new AttachEvidenceResult(evidenceId, evidence.Sha256.Value, storedPath);
    }

    private static DateTime EnsureUtc(DateTime dt)
        => dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
