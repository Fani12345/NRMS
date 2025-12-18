namespace NRMS.Application.Contracts.Numbering;

public sealed record AllocateNumberingBlockCommand(
    Guid BlockId,
    Guid OperatorId,
    DateTime? ExpiresAtUtc,
    string? Notes
);
