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

    public Task<NrmsCase?> GetByReferenceAsync(string caseReference, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(caseReference))
            return Task.FromResult<NrmsCase?>(null);

        var target = caseReference.Trim();

        // Assumes you store cases in a collection/dictionary. If your field name differs,
        // update only the _cases reference below to match your existing field.
        var found = _cases.Values.FirstOrDefault(c =>
            string.Equals(c.CaseReference, target, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(found);
    }

}
