using NRMS.Application.Abstractions;
using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryNumberingBlockRepository : INumberingBlockRepository
{
    private readonly List<NumberingBlock> _blocks = new();

    public Task AddAsync(NumberingBlock block, CancellationToken ct = default)
    {
        if (block is null) throw new ArgumentNullException(nameof(block));

        if (_blocks.Any(b => b.BlockId == block.BlockId))
            throw new InvalidOperationException("Block already exists.");

        _blocks.Add(block);
        return Task.CompletedTask;
    }

    public Task<NumberingBlock?> GetAsync(Guid blockId, CancellationToken ct = default)
    {
        var block = _blocks.SingleOrDefault(b => b.BlockId == blockId);
        return Task.FromResult(block);
    }

    public Task<IReadOnlyList<NumberingBlock>> ListByTypeAsync(NumberType numberType, CancellationToken ct = default)
    {
        IReadOnlyList<NumberingBlock> result = _blocks.Where(b => b.NumberType == numberType).ToList();
        return Task.FromResult(result);
    }

    public Task UpdateAsync(NumberingBlock block, CancellationToken ct = default)
    {
        if (block is null) throw new ArgumentNullException(nameof(block));

        var idx = _blocks.FindIndex(b => b.BlockId == block.BlockId);
        if (idx < 0) throw new InvalidOperationException("Block not found.");

        _blocks[idx] = block;
        return Task.CompletedTask;
    }
}
