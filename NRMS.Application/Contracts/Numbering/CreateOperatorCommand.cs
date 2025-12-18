namespace NRMS.Application.Contracts.Numbering;

public sealed record CreateOperatorCommand(
    string Name,
    string? LicenseNumber
);
