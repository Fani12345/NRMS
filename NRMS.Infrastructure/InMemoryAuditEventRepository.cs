using NRMS.Application.Abstractions;
using NRMS.Domain.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryAuditEventRepository : IAuditEventRepository
{
    private readonly List<AuditEvent> _events = new();

    public Task AppendAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        if (auditEvent is null) throw new ArgumentNullException(nameof(auditEvent));

        _events.Add(auditEvent);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEvent>> GetByCaseAsync(Guid caseId, CancellationToken ct = default)
    {
        if (caseId == Guid.Empty)
            return Task.FromResult<IReadOnlyList<AuditEvent>>(Array.Empty<AuditEvent>());

        IReadOnlyList<AuditEvent> result = _events
            .Where(e => e.CaseId == caseId)
            .OrderBy(e => e.OccurredAtUtc)
            .ToList();

        return Task.FromResult(result);
    }
}
