using NRMS.Domain.Numbering.Entities;

namespace NRMS.Application.Abstractions;

public interface IAllocationRepository
{
    Task AddAsync(Allocation allocation, CancellationToken ct = default);

    Task<Allocation?> GetAsync(Guid allocationId, CancellationToken ct = default);

    Task<IReadOnlyList<Allocation>> ListByBlockAsync(Guid blockId, CancellationToken ct = default);

    Task<IReadOnlyList<Allocation>> ListByOperatorAsync(Guid operatorId, CancellationToken ct = default);
}
