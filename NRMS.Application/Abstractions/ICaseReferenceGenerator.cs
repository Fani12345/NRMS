using NRMS.Domain.Enums;

namespace NRMS.Application.Abstractions;

public interface ICaseReferenceGenerator
{
    string Next(CaseType caseType, DateTime utcNow);
}
