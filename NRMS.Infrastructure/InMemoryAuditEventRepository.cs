using NRMS.Application.Abstractions;
using NRMS.Domain.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryAuditEventRepository : IAuditEventRepository
{
    private readonly List<AuditEvent> _events = new();

    public Task AppendAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        _events.Add(auditEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEvent>> GetByCaseAsync(Guid caseId, CancellationToken ct = default)
    {
        IReadOnlyList<AuditEvent> result = _events.Where(e => e.CaseId == caseId).ToList();
        return Task.FromResult(result);
    }
}

