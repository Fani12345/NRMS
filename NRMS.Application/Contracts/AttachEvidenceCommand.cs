namespace NRMS.Application.Contracts;

public sealed record AttachEvidenceCommand(
    Guid CaseId,
    string ImportedBy,
    string FileName,
    string SourceDescription,
    Stream Content);

public sealed record AttachEvidenceResult(Guid EvidenceId, string Sha256, string StoredPath);
