namespace NRMS.Application.Abstractions;

public interface IHashService
{
    Task<string> ComputeSha256Async(Stream content, CancellationToken ct = default);
}
