using NRMS.Domain.Numbering.Entities;

namespace NRMS.Tests.Numbering;

public sealed class AllocationTests
{
    [Fact]
    public void Create_WhenValid_SetsFields()
    {
        var utcNow = DateTime.UtcNow;

        var a = Allocation.Create(
            blockId: Guid.NewGuid(),
            operatorId: Guid.NewGuid(),
            utcNow: utcNow,
            expiresAtUtc: utcNow.AddDays(30),
            notes: "Allocated for rollout");

        Assert.NotEqual(Guid.Empty, a.AllocationId);
        Assert.True(a.ExpiresAtUtc.HasValue);
        Assert.True(a.ExpiresAtUtc!.Value > a.AllocatedAtUtc);
    }

    [Fact]
    public void Constructor_WhenExpiresBeforeAllocated_Throws()
    {
        var t = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new Allocation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), t, t.AddMinutes(-1)));
    }

    [Fact]
    public void Constructor_WhenIdsEmpty_Throws()
    {
        var t = DateTime.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            new Allocation(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), t, null));

        Assert.Throws<ArgumentException>(() =>
            new Allocation(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), t, null));

        Assert.Throws<ArgumentException>(() =>
            new Allocation(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, t, null));
    }
}
