using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Tests.Numbering;

public sealed class NumberRangeTests
{
    [Fact]
    public void Create_WhenEndLessThanStart_Throws()
    {
        Assert.Throws<ArgumentException>(() => NumberRange.Create(200, 100));
    }

    [Fact]
    public void Create_WhenStartNonPositive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NumberRange.Create(0, 100));
        Assert.Throws<ArgumentOutOfRangeException>(() => NumberRange.Create(-1, 100));
    }

    [Fact]
    public void Create_WhenEndNonPositive_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => NumberRange.Create(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => NumberRange.Create(1, -1));
    }

    [Fact]
    public void Contains_ValueWithinRange_ReturnsTrue()
    {
        var r = NumberRange.Create(100, 200);

        Assert.True(r.Contains(100));
        Assert.True(r.Contains(150));
        Assert.True(r.Contains(200));
        Assert.False(r.Contains(99));
        Assert.False(r.Contains(201));
    }

    [Fact]
    public void Contains_RangeWithinRange_ReturnsTrue()
    {
        var outer = NumberRange.Create(100, 200);
        var inner = NumberRange.Create(120, 180);

        Assert.True(outer.Contains(inner));
        Assert.False(inner.Contains(outer));
    }

    [Fact]
    public void Overlaps_WhenOverlapping_ReturnsTrue()
    {
        var a = NumberRange.Create(100, 200);
        var b = NumberRange.Create(150, 250);

        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_WhenTouchingAtBoundary_ReturnsTrue()
    {
        var a = NumberRange.Create(100, 200);
        var b = NumberRange.Create(200, 300);

        Assert.True(a.Overlaps(b));
        Assert.True(b.Overlaps(a));
    }

    [Fact]
    public void Overlaps_WhenDisjoint_ReturnsFalse()
    {
        var a = NumberRange.Create(100, 199);
        var b = NumberRange.Create(200, 300);

        Assert.False(a.Overlaps(b));
        Assert.False(b.Overlaps(a));
    }
}
