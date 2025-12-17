using NRMS.Domain.Entities;

namespace NRMS.Application.Abstractions;

public interface ICaseRepository
{
    Task AddAsync(NrmsCase nrmsCase, CancellationToken ct = default);
    Task<NrmsCase?> GetAsync(Guid caseId, CancellationToken ct = default);
    Task UpdateAsync(NrmsCase nrmsCase, CancellationToken ct = default);
}
