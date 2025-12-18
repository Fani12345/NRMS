namespace NRMS.Domain.Numbering.Entities;

public sealed class Allocation
{
    public Guid AllocationId { get; }
    public Guid BlockId { get; }
    public Guid OperatorId { get; }
    public DateTime AllocatedAtUtc { get; }
    public DateTime? ExpiresAtUtc { get; }
    public string? Notes { get; }

    public Allocation(
        Guid allocationId,
        Guid blockId,
        Guid operatorId,
        DateTime allocatedAtUtc,
        DateTime? expiresAtUtc,
        string? notes = null)
    {
        if (allocationId == Guid.Empty) throw new ArgumentException("AllocationId is required.", nameof(allocationId));
        if (blockId == Guid.Empty) throw new ArgumentException("BlockId is required.", nameof(blockId));
        if (operatorId == Guid.Empty) throw new ArgumentException("OperatorId is required.", nameof(operatorId));

        var a = allocatedAtUtc.Kind == DateTimeKind.Utc ? allocatedAtUtc : DateTime.SpecifyKind(allocatedAtUtc, DateTimeKind.Utc);
        DateTime? e = null;

        if (expiresAtUtc.HasValue)
        {
            e = expiresAtUtc.Value.Kind == DateTimeKind.Utc ? expiresAtUtc.Value : DateTime.SpecifyKind(expiresAtUtc.Value, DateTimeKind.Utc);
            if (e.Value <= a) throw new ArgumentException("ExpiresAtUtc must be after AllocatedAtUtc.", nameof(expiresAtUtc));
        }

        AllocationId = allocationId;
        BlockId = blockId;
        OperatorId = operatorId;
        AllocatedAtUtc = a;
        ExpiresAtUtc = e;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public static Allocation Create(Guid blockId, Guid operatorId, DateTime utcNow, DateTime? expiresAtUtc = null, string? notes = null)
        => new(Guid.NewGuid(), blockId, operatorId, utcNow, expiresAtUtc, notes);
}
