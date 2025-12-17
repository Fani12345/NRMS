using NRMS.Application.Abstractions;
using NRMS.Domain.Enums;

namespace NRMS.Infrastructure;

public sealed class SimpleCaseReferenceGenerator : ICaseReferenceGenerator
{
    private int _sequence = 0;

    public string Next(CaseType caseType, DateTime utcNow)
    {
        var year = utcNow.Year;
        var next = Interlocked.Increment(ref _sequence);

        var prefix = caseType switch
        {
            CaseType.Allocation => "CASE",
            CaseType.Audit => "AUD",
            CaseType.Reclamation => "REC",
            CaseType.Correction => "COR",
            _ => "CASE"
        };

        return $"{prefix}-{year}-{next:0000}";
    }
}
