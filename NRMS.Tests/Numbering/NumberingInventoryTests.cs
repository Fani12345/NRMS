using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Tests.Numbering;

public sealed class NumberingInventoryTests
{
    [Fact]
    public void AddBlock_WhenFirstBlock_AddsSuccessfully()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);

        var b = inv.AddBlock(NumberRange.Create(60000000, 60009999), "Initial inventory");

        Assert.Single(inv.Blocks);
        Assert.Equal(NumberType.Msisdn, b.NumberType);
        Assert.Equal(BlockStatus.Available, b.Status);
    }

    [Fact]
    public void AddBlock_WhenDisjoint_AddsSuccessfully()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);

        inv.AddBlock(NumberRange.Create(60000000, 60009999));
        inv.AddBlock(NumberRange.Create(60010000, 60019999));

        Assert.Equal(2, inv.Blocks.Count);
    }

    [Fact]
    public void AddBlock_WhenOverlapping_Throws()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);

        inv.AddBlock(NumberRange.Create(60000000, 60009999));

        Assert.Throws<InvalidOperationException>(() =>
            inv.AddBlock(NumberRange.Create(60005000, 60015000)));
    }

    [Fact]
    public void Create_WhenNumberTypeUnknown_Throws()
    {
        Assert.Throws<ArgumentException>(() => new NumberingInventory(Guid.NewGuid(), NumberType.Unknown));
    }

    [Fact]
    public void Constructor_WhenInventoryIdEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new NumberingInventory(Guid.Empty, NumberType.Msisdn));
    }
}
