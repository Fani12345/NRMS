using NRMS.Application.Abstractions;
using NRMS.Domain.Numbering.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryAllocationRepository : IAllocationRepository
{
    private readonly List<Allocation> _allocations = new();

    public Task AddAsync(Allocation allocation, CancellationToken ct = default)
    {
        if (allocation is null) throw new ArgumentNullException(nameof(allocation));

        if (_allocations.Any(a => a.AllocationId == allocation.AllocationId))
            throw new InvalidOperationException("Allocation already exists.");

        _allocations.Add(allocation);
        return Task.CompletedTask;
    }

    public Task<Allocation?> GetAsync(Guid allocationId, CancellationToken ct = default)
    {
        var a = _allocations.SingleOrDefault(x => x.AllocationId == allocationId);
        return Task.FromResult(a);
    }

    public Task<IReadOnlyList<Allocation>> ListByBlockAsync(Guid blockId, CancellationToken ct = default)
    {
        IReadOnlyList<Allocation> result = _allocations.Where(a => a.BlockId == blockId).ToList();
        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<Allocation>> ListByOperatorAsync(Guid operatorId, CancellationToken ct = default)
    {
        IReadOnlyList<Allocation> result = _allocations.Where(a => a.OperatorId == operatorId).ToList();
        return Task.FromResult(result);
    }
}
