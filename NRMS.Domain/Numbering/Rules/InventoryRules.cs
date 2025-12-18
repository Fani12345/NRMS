using NRMS.Domain.Numbering.Entities;

namespace NRMS.Domain.Numbering.Rules;

public static class InventoryRules
{
    public static void EnsureNoOverlap(NumberingBlock candidate, IEnumerable<NumberingBlock> existing)
    {
        if (candidate is null) throw new ArgumentNullException(nameof(candidate));
        if (existing is null) throw new ArgumentNullException(nameof(existing));

        foreach (var b in existing)
        {
            if (b.NumberType != candidate.NumberType)
                continue;

            if (b.BlockId == candidate.BlockId)
                continue;

            if (b.Range.Overlaps(candidate.Range))
                throw new InvalidOperationException($"Overlapping inventory blocks detected: {b.Range} overlaps {candidate.Range}.");
        }
    }
}
