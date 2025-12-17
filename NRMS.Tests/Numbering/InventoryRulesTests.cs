using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Domain.Numbering.Rules;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Tests.Numbering;

public sealed class InventoryRulesTests
{
    [Fact]
    public void EnsureNoOverlap_WhenSameTypeOverlaps_Throws()
    {
        var existing = new List<NumberingBlock>
        {
            new NumberingBlock(Guid.NewGuid(), NumberType.Msisdn, NumberRange.Create(60000000, 60009999), BlockStatus.Available),
        };

        var candidate = new NumberingBlock(Guid.NewGuid(), NumberType.Msisdn, NumberRange.Create(60005000, 60015000), BlockStatus.Available);

        Assert.Throws<InvalidOperationException>(() => InventoryRules.EnsureNoOverlap(candidate, existing));
    }

    [Fact]
    public void EnsureNoOverlap_WhenDifferentTypeOverlaps_DoesNotThrow()
    {
        var existing = new List<NumberingBlock>
        {
            new NumberingBlock(Guid.NewGuid(), NumberType.Msisdn, NumberRange.Create(60000000, 60009999), BlockStatus.Available),
        };

        var candidate = new NumberingBlock(Guid.NewGuid(), NumberType.ShortCode, NumberRange.Create(60005000, 60015000), BlockStatus.Available);

        InventoryRules.EnsureNoOverlap(candidate, existing);
    }

    [Fact]
    public void EnsureNoOverlap_WhenDisjoint_DoesNotThrow()
    {
        var existing = new List<NumberingBlock>
        {
            new NumberingBlock(Guid.NewGuid(), NumberType.Msisdn, NumberRange.Create(60000000, 60009999), BlockStatus.Available),
        };

        var candidate = new NumberingBlock(Guid.NewGuid(), NumberType.Msisdn, NumberRange.Create(60010000, 60019999), BlockStatus.Available);

        InventoryRules.EnsureNoOverlap(candidate, existing);
    }
}
