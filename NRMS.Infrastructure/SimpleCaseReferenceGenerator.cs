using NRMS.Application.Abstractions;
using NRMS.Domain.Enums;
using System.Threading;

namespace NRMS.Infrastructure;

public sealed class SimpleCaseReferenceGenerator : ICaseReferenceGenerator
{
    private int _sequence;

    // lastUsedSequence is what already exists in the database for the given year
    public SimpleCaseReferenceGenerator(int lastUsedSequence = 0)
    {
        _sequence = lastUsedSequence < 0 ? 0 : lastUsedSequence;
    }

    // Implements: ICaseReferenceGenerator.Next(CaseType, DateTime)
    public string Next(CaseType caseType, DateTime utcNow)
    {
        var ts = utcNow.Kind == DateTimeKind.Utc ? utcNow : DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

        // Keep one monotonic sequence for the year (simple + reliable)
        var next = Interlocked.Increment(ref _sequence);

        // Optional: include case type initial to help humans (doesn't affect uniqueness)
        // e.g., A=Allocation, R=Reclamation, C=Correction, etc.
        var typeCode = GetTypeCode(caseType);

        return $"CASE-{ts.Year}-{typeCode}{next:0000}";
    }

    private static string GetTypeCode(CaseType caseType)
    {
        // Keep this stable; change only if you want different human-readable codes.
        return caseType switch
        {
            CaseType.Allocation => "A",
            CaseType.Reclamation => "R",
            CaseType.Correction => "C",
            _ => "X"
        };
    }
}
