using NRMS.Application.Abstractions;
using NRMS.Domain.Entities;
using NRMS.Domain.Enums;

namespace NRMS.Infrastructure.Persistence.Sqlite;

public sealed class SqliteAuditEventRepository : IAuditEventRepository
{
    private readonly SqliteDb _db;

    public SqliteAuditEventRepository(SqliteDb db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _db.EnsureCreated();
    }

    public async Task AppendAsync(AuditEvent auditEvent, CancellationToken ct = default)
    {
        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO AuditEvents (AuditEventId, CaseId, EventType, OccurredAtUtc, Actor, ObjectType, ObjectId, DetailsJson)
VALUES (@id, @caseId, @type, @at, @actor, @objType, @objId, @json);
";
        cmd.Parameters.AddWithValue("@id", auditEvent.AuditEventId.ToString("D"));
        cmd.Parameters.AddWithValue("@caseId", auditEvent.CaseId.ToString("D"));
        cmd.Parameters.AddWithValue("@type", (int)auditEvent.EventType);
        cmd.Parameters.AddWithValue("@at", auditEvent.OccurredAtUtc.ToString("O"));
        cmd.Parameters.AddWithValue("@actor", auditEvent.Actor);
        cmd.Parameters.AddWithValue("@objType", auditEvent.ObjectType);
        cmd.Parameters.AddWithValue("@objId", auditEvent.ObjectId);
        cmd.Parameters.AddWithValue("@json", auditEvent.DetailsJson);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    public async Task<IReadOnlyList<AuditEvent>> GetByCaseAsync(Guid caseId, CancellationToken ct = default)
    {
        var result = new List<AuditEvent>();

        await using var conn = _db.OpenConnection();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT AuditEventId, EventType, OccurredAtUtc, Actor, ObjectType, ObjectId, DetailsJson
FROM AuditEvents
WHERE CaseId = @caseId
ORDER BY OccurredAtUtc ASC;
";
        cmd.Parameters.AddWithValue("@caseId", caseId.ToString("D"));

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var auditEventId = Guid.Parse(reader.GetString(0));
            var eventType = (AuditEventType)reader.GetInt32(1);
            var occurredAtUtc = DateTime.SpecifyKind(DateTime.Parse(reader.GetString(2)), DateTimeKind.Utc);
            var actor = reader.GetString(3);
            var objectType = reader.GetString(4);
            var objectId = reader.GetString(5);
            var detailsJson = reader.GetString(6);

            result.Add(new AuditEvent(
                auditEventId,
                eventType,
                occurredAtUtc,
                actor,
                caseId,
                objectType,
                objectId,
                detailsJson
            ));
        }

        return result;
    }
}
