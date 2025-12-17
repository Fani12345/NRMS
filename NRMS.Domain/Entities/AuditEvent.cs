using NRMS.Domain.Enums;
using NRMS.Domain.Exceptions;

namespace NRMS.Domain.Entities;

public sealed class AuditEvent
{
    public Guid AuditEventId { get; }
    public AuditEventType EventType { get; }
    public DateTime OccurredAtUtc { get; }
    public string Actor { get; }
    public Guid CaseId { get; }
    public string ObjectType { get; }
    public string ObjectId { get; }
    public string DetailsJson { get; }

    public AuditEvent(
        Guid auditEventId,
        AuditEventType eventType,
        DateTime occurredAtUtc,
        string actor,
        Guid caseId,
        string objectType,
        string objectId,
        string detailsJson)
    {
        if (auditEventId == Guid.Empty) throw new DomainException("AuditEventId is required.");
        if (occurredAtUtc.Kind != DateTimeKind.Utc) throw new DomainException("OccurredAtUtc must be UTC.");
        if (string.IsNullOrWhiteSpace(actor)) throw new DomainException("Actor is required.");
        if (caseId == Guid.Empty) throw new DomainException("CaseId is required.");
        if (string.IsNullOrWhiteSpace(objectType)) throw new DomainException("ObjectType is required.");
        if (string.IsNullOrWhiteSpace(objectId)) throw new DomainException("ObjectId is required.");

        AuditEventId = auditEventId;
        EventType = eventType;
        OccurredAtUtc = occurredAtUtc;
        Actor = actor.Trim();
        CaseId = caseId;
        ObjectType = objectType.Trim();
        ObjectId = objectId.Trim();
        DetailsJson = (detailsJson ?? "{}").Trim();
    }
}
