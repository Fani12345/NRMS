using NRMS.Domain.Exceptions;
using NRMS.Domain.ValueObjects;

namespace NRMS.Domain.Entities;

public sealed class EvidenceItem
{
    public Guid EvidenceId { get; }
    public string FileName { get; }
    public string SourceDescription { get; }
    public DateTime ImportedAtUtc { get; }
    public string ImportedBy { get; }
    public Sha256 Sha256 { get; }
    public string StoredPath { get; }

    public EvidenceItem(
        Guid evidenceId,
        string fileName,
        string sourceDescription,
        DateTime importedAtUtc,
        string importedBy,
        Sha256 sha256,
        string storedPath)
    {
        if (evidenceId == Guid.Empty) throw new DomainException("EvidenceId is required.");
        if (string.IsNullOrWhiteSpace(fileName)) throw new DomainException("FileName is required.");
        if (string.IsNullOrWhiteSpace(importedBy)) throw new DomainException("ImportedBy is required.");
        if (string.IsNullOrWhiteSpace(storedPath)) throw new DomainException("StoredPath is required.");
        if (importedAtUtc.Kind != DateTimeKind.Utc) throw new DomainException("ImportedAtUtc must be UTC.");

        EvidenceId = evidenceId;
        FileName = fileName.Trim();
        SourceDescription = (sourceDescription ?? string.Empty).Trim();
        ImportedAtUtc = importedAtUtc;
        ImportedBy = importedBy.Trim();
        Sha256 = sha256;
        StoredPath = storedPath.Trim();
    }
}
