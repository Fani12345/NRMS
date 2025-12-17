using NRMS.Application.Abstractions;
using NRMS.Domain.Numbering.Entities;

namespace NRMS.Infrastructure;

public sealed class InMemoryOperatorRepository : IOperatorRepository
{
    private readonly List<Operator> _operators = new();

    public Task AddAsync(Operator op, CancellationToken ct = default)
    {
        if (op is null) throw new ArgumentNullException(nameof(op));

        if (_operators.Any(o => o.OperatorId == op.OperatorId))
            throw new InvalidOperationException("Operator already exists.");

        _operators.Add(op);
        return Task.CompletedTask;
    }

    public Task<Operator?> GetAsync(Guid operatorId, CancellationToken ct = default)
    {
        var op = _operators.SingleOrDefault(o => o.OperatorId == operatorId);
        return Task.FromResult(op);
    }

    public Task<Operator?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Task.FromResult<Operator?>(null);

        var op = _operators.SingleOrDefault(o =>
            string.Equals(o.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(op);
    }

    public Task<IReadOnlyList<Operator>> ListAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Operator> result = _operators.ToList();
        return Task.FromResult(result);
    }
}
