using NRMS.Domain.Numbering.Rules;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Domain.Numbering.Entities;

public sealed class NumberingInventory
{
    private readonly List<NumberingBlock> _blocks = new();
    private readonly List<Allocation> _allocations = new();

    public Guid InventoryId { get; }
    public NumberType NumberType { get; }

    public IReadOnlyList<NumberingBlock> Blocks => _blocks;
    public IReadOnlyList<Allocation> Allocations => _allocations;

    public NumberingInventory(Guid inventoryId, NumberType numberType)
    {
        if (inventoryId == Guid.Empty) throw new ArgumentException("InventoryId is required.", nameof(inventoryId));
        if (numberType == NumberType.Unknown) throw new ArgumentException("NumberType is required.", nameof(numberType));

        InventoryId = inventoryId;
        NumberType = numberType;
    }

    public static NumberingInventory Create(NumberType numberType)
        => new(Guid.NewGuid(), numberType);

    public NumberingBlock AddBlock(NumberRange range, string? notes = null)
    {
        var candidate = NumberingBlock.Create(NumberType, range, notes);

        // Core rule: no overlaps within same NumberType inventory.
        InventoryRules.EnsureNoOverlap(candidate, _blocks);

        _blocks.Add(candidate);
        return candidate;
    }

    public Allocation AllocateBlock(Guid blockId, Guid operatorId, DateTime utcNow, DateTime? expiresAtUtc = null, string? notes = null)
    {
        if (blockId == Guid.Empty) throw new ArgumentException("BlockId is required.", nameof(blockId));
        if (operatorId == Guid.Empty) throw new ArgumentException("OperatorId is required.", nameof(operatorId));

        var block = _blocks.SingleOrDefault(b => b.BlockId == blockId);
        if (block is null)
            throw new InvalidOperationException($"Block not found: {blockId}");

        // Disallow allocating same block twice.
        if (_allocations.Any(a => a.BlockId == blockId))
            throw new InvalidOperationException("Block is already allocated.");

        // Change block state (enforces allowed transitions internally)
        block.Allocate(notes);

        var allocation = Allocation.Create(
            blockId: blockId,
            operatorId: operatorId,
            utcNow: utcNow,
            expiresAtUtc: expiresAtUtc,
            notes: notes);

        _allocations.Add(allocation);
        return allocation;
    }
}
