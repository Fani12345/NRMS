using NRMS.Domain.Enums;

namespace NRMS.Application.Contracts;

public sealed record CreateCaseCommand(CaseType CaseType, string CreatedBy);
public sealed record CreateCaseResult(Guid CaseId, string CaseReference);
