using NRMS.Domain.Entities;

namespace NRMS.Application.Abstractions;

public interface IAuditEventRepository
{
    Task AppendAsync(AuditEvent auditEvent, CancellationToken ct = default);
    Task<IReadOnlyList<AuditEvent>> GetByCaseAsync(Guid caseId, CancellationToken ct = default);
}
