using NRMS.Domain.Enums;
using NRMS.Domain.Exceptions;

namespace NRMS.Domain.Entities;

public sealed class NrmsCase
{
    private readonly List<EvidenceItem> _evidence = new();

    public Guid CaseId { get; }
    public string CaseReference { get; }
    public CaseType CaseType { get; }
    public CaseStatus Status { get; private set; }
    public string CreatedBy { get; }
    public DateTime CreatedAtUtc { get; }

    public IReadOnlyList<EvidenceItem> Evidence => _evidence.AsReadOnly();

    public NrmsCase(
        Guid caseId,
        string caseReference,
        CaseType caseType,
        string createdBy,
        DateTime createdAtUtc)
    {
        if (caseId == Guid.Empty) throw new DomainException("CaseId is required.");
        if (string.IsNullOrWhiteSpace(caseReference)) throw new DomainException("CaseReference is required.");
        if (string.IsNullOrWhiteSpace(createdBy)) throw new DomainException("CreatedBy is required.");
        if (createdAtUtc.Kind != DateTimeKind.Utc) throw new DomainException("CreatedAtUtc must be UTC.");

        CaseId = caseId;
        CaseReference = caseReference.Trim();
        CaseType = caseType;
        CreatedBy = createdBy.Trim();
        CreatedAtUtc = createdAtUtc;
        Status = CaseStatus.Draft;
    }

    public void AddEvidence(EvidenceItem item)
    {
        if (item is null) throw new DomainException("Evidence item is required.");

        if (_evidence.Any(e => e.EvidenceId == item.EvidenceId))
            throw new DomainException("Evidence item already exists.");

        _evidence.Add(item);
    }
}
