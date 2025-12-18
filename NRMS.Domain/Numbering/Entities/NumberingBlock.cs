using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Domain.Numbering.Entities;

public enum BlockStatus
{
    Available = 0,
    Reserved = 1,
    Allocated = 2,
    Retired = 3
}

public sealed class NumberingBlock
{
    public Guid BlockId { get; }
    public NumberType NumberType { get; }
    public NumberRange Range { get; }
    public BlockStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public NumberingBlock(Guid blockId, NumberType numberType, NumberRange range, BlockStatus status, string? notes = null)
    {
        if (blockId == Guid.Empty) throw new ArgumentException("BlockId is required.", nameof(blockId));
        if (numberType == NumberType.Unknown) throw new ArgumentException("NumberType is required.", nameof(numberType));

        BlockId = blockId;
        NumberType = numberType;
        Range = range;
        Status = status;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public static NumberingBlock Create(NumberType numberType, NumberRange range, string? notes = null)
        => new(Guid.NewGuid(), numberType, range, BlockStatus.Available, notes);

    public void Reserve(string? notes = null)
    {
        if (Status != BlockStatus.Available)
            throw new InvalidOperationException($"Cannot reserve block in status {Status}.");

        Status = BlockStatus.Reserved;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }

    public void Allocate(string? notes = null)
    {
        if (Status != BlockStatus.Available && Status != BlockStatus.Reserved)
            throw new InvalidOperationException($"Cannot allocate block in status {Status}.");

        Status = BlockStatus.Allocated;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }

    public void Retire(string? notes = null)
    {
        if (Status == BlockStatus.Retired)
            return;

        Status = BlockStatus.Retired;
        Notes = string.IsNullOrWhiteSpace(notes) ? Notes : notes.Trim();
    }
}
