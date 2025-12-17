using NRMS.Domain.Numbering.Entities;

namespace NRMS.Application.Abstractions;

public interface IOperatorRepository
{
    Task AddAsync(Operator op, CancellationToken ct = default);

    Task<Operator?> GetAsync(Guid operatorId, CancellationToken ct = default);

    Task<Operator?> GetByNameAsync(string name, CancellationToken ct = default);

    Task<IReadOnlyList<Operator>> ListAsync(CancellationToken ct = default);
}
