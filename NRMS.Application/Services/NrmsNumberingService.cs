using NRMS.Application.Abstractions;
using NRMS.Application.Contracts.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Domain.Numbering.Rules;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Application.Services;

public sealed class NrmsNumberingService
{
    private readonly IOperatorRepository _operators;
    private readonly INumberingBlockRepository _blocks;
    private readonly IAllocationRepository _allocations;

    public NrmsNumberingService(
        IOperatorRepository operators,
        INumberingBlockRepository blocks,
        IAllocationRepository allocations)
    {
        _operators = operators ?? throw new ArgumentNullException(nameof(operators));
        _blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        _allocations = allocations ?? throw new ArgumentNullException(nameof(allocations));
    }

    public async Task<CreateOperatorResult> CreateOperatorAsync(CreateOperatorCommand cmd, CancellationToken ct = default)
    {
        if (cmd is null) throw new ArgumentNullException(nameof(cmd));

        var op = Operator.Create(cmd.Name, cmd.LicenseNumber);

        // Optional: prevent duplicates by name (case-insensitive).
        var existing = await _operators.GetByNameAsync(op.Name, ct);
        if (existing is not null)
            throw new InvalidOperationException("Operator already exists (same name).");

        await _operators.AddAsync(op, ct);
        return new CreateOperatorResult(op.OperatorId);
    }

    public async Task<AddNumberingBlockResult> AddNumberingBlockAsync(AddNumberingBlockCommand cmd, CancellationToken ct = default)
    {
        if (cmd is null) throw new ArgumentNullException(nameof(cmd));

        var range = NumberRange.Create(cmd.Start, cmd.End);
        var candidate = NumberingBlock.Create(cmd.NumberType, range, cmd.Notes);

        var existing = await _blocks.ListByTypeAsync(cmd.NumberType, ct);
        InventoryRules.EnsureNoOverlap(candidate, existing);

        await _blocks.AddAsync(candidate, ct);
        return new AddNumberingBlockResult(candidate.BlockId);
    }

    public async Task<AllocateNumberingBlockResult> AllocateNumberingBlockAsync(AllocateNumberingBlockCommand cmd, CancellationToken ct = default)
    {
        if (cmd is null) throw new ArgumentNullException(nameof(cmd));
        if (cmd.BlockId == Guid.Empty) throw new ArgumentException("BlockId is required.", nameof(cmd.BlockId));
        if (cmd.OperatorId == Guid.Empty) throw new ArgumentException("OperatorId is required.", nameof(cmd.OperatorId));

        var block = await _blocks.GetAsync(cmd.BlockId, ct);
        if (block is null)
            throw new InvalidOperationException("Block not found.");

        var operatorEntity = await _operators.GetAsync(cmd.OperatorId, ct);
        if (operatorEntity is null)
            throw new InvalidOperationException("Operator not found.");

        var prior = await _allocations.ListByBlockAsync(cmd.BlockId, ct);
        if (prior.Count > 0)
            throw new InvalidOperationException("Block is already allocated.");

        // Changes domain state (validated inside entity)
        block.Allocate(cmd.Notes);
        await _blocks.UpdateAsync(block, ct);

        var allocation = Allocation.Create(
            blockId: cmd.BlockId,
            operatorId: cmd.OperatorId,
            utcNow: DateTime.UtcNow,
            expiresAtUtc: cmd.ExpiresAtUtc,
            notes: cmd.Notes);

        await _allocations.AddAsync(allocation, ct);

        return new AllocateNumberingBlockResult(allocation.AllocationId);
    }
}
