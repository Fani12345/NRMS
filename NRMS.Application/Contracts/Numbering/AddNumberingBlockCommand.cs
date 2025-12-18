using NRMS.Domain.Numbering;

namespace NRMS.Application.Contracts.Numbering;

public sealed record AddNumberingBlockCommand(
    NumberType NumberType,
    long Start,
    long End,
    string? Notes
);
