using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;

namespace NRMS.Application.Abstractions;

public interface INumberingBlockRepository
{
    Task AddAsync(NumberingBlock block, CancellationToken ct = default);

    Task<NumberingBlock?> GetAsync(Guid blockId, CancellationToken ct = default);

    Task<IReadOnlyList<NumberingBlock>> ListByTypeAsync(NumberType numberType, CancellationToken ct = default);

    Task UpdateAsync(NumberingBlock block, CancellationToken ct = default);
}
