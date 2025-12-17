namespace NRMS.Application.Abstractions;

public interface IEvidenceStorage
{
    Task<string> SaveAsync(Guid evidenceId, string originalFileName, Stream content, CancellationToken ct = default);
}
