using NRMS.Application.Abstractions;
using NRMS.Domain.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryCaseRepository : ICaseRepository
{
    private readonly Dictionary<Guid, NrmsCase> _cases = new();

    public Task AddAsync(NrmsCase nrmsCase, CancellationToken ct = default)
    {
        _cases.Add(nrmsCase.CaseId, nrmsCase);
        return Task.CompletedTask;
    }

    public Task<NrmsCase?> GetAsync(Guid caseId, CancellationToken ct = default)
    {
        _cases.TryGetValue(caseId, out var c);
        return Task.FromResult(c);
    }

    public Task UpdateAsync(NrmsCase nrmsCase, CancellationToken ct = default)
    {
        _cases[nrmsCase.CaseId] = nrmsCase;
        return Task.CompletedTask;
    }
}
