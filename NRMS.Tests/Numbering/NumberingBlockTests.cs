using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Tests.Numbering;

public sealed class NumberingBlockTests
{
    [Fact]
    public void Create_DefaultsToAvailable()
    {
        var range = NumberRange.Create(60000000, 60009999);
        var block = NumberingBlock.Create(NumberType.Msisdn, range, notes: "Initial inventory");

        Assert.Equal(BlockStatus.Available, block.Status);
        Assert.Equal(NumberType.Msisdn, block.NumberType);
        Assert.Equal(range, block.Range);
    }

    [Fact]
    public void Reserve_FromAvailable_Succeeds()
    {
        var block = NumberingBlock.Create(NumberType.Msisdn, NumberRange.Create(60000000, 60009999));

        block.Reserve("Reserved pending review");

        Assert.Equal(BlockStatus.Reserved, block.Status);
    }

    [Fact]
    public void Allocate_FromAvailable_Succeeds()
    {
        var block = NumberingBlock.Create(NumberType.Msisdn, NumberRange.Create(60000000, 60009999));

        block.Allocate("Allocated to operator");

        Assert.Equal(BlockStatus.Allocated, block.Status);
    }

    [Fact]
    public void Allocate_FromReserved_Succeeds()
    {
        var block = NumberingBlock.Create(NumberType.Msisdn, NumberRange.Create(60000000, 60009999));
        block.Reserve("Reserved");

        block.Allocate("Allocated");

        Assert.Equal(BlockStatus.Allocated, block.Status);
    }

    [Fact]
    public void Reserve_FromAllocated_Throws()
    {
        var block = NumberingBlock.Create(NumberType.Msisdn, NumberRange.Create(60000000, 60009999));
        block.Allocate("Allocated");

        Assert.Throws<InvalidOperationException>(() => block.Reserve("Attempt invalid reserve"));
    }

    [Fact]
    public void Retire_FromAnyStatus_SetsRetired()
    {
        var block = NumberingBlock.Create(NumberType.Msisdn, NumberRange.Create(60000000, 60009999));
        block.Reserve("Reserved");

        block.Retire("Retired");

        Assert.Equal(BlockStatus.Retired, block.Status);
    }

    [Fact]
    public void Constructor_WhenNumberTypeUnknown_Throws()
    {
        var range = NumberRange.Create(100, 200);

        Assert.Throws<ArgumentException>(() =>
            new NumberingBlock(Guid.NewGuid(), NumberType.Unknown, range, BlockStatus.Available));
    }

    [Fact]
    public void Constructor_WhenBlockIdEmpty_Throws()
    {
        var range = NumberRange.Create(100, 200);

        Assert.Throws<ArgumentException>(() =>
            new NumberingBlock(Guid.Empty, NumberType.Msisdn, range, BlockStatus.Available));
    }
}
