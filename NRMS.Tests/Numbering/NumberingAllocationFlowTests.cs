using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Domain.Numbering.ValueObjects;

namespace NRMS.Tests.Numbering;

public sealed class NumberingAllocationFlowTests
{
    [Fact]
    public void AllocateBlock_WhenAvailableBlock_AllocatesAndCreatesAllocation()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);
        var block = inv.AddBlock(NumberRange.Create(60000000, 60009999), "Inventory");

        var op = Operator.Create("Vodacom", "LIC-001");
        var now = DateTime.UtcNow;

        var allocation = inv.AllocateBlock(block.BlockId, op.OperatorId, now, expiresAtUtc: now.AddDays(30), notes: "Rollout");

        Assert.Equal(block.BlockId, allocation.BlockId);
        Assert.Equal(op.OperatorId, allocation.OperatorId);
        Assert.Equal(BlockStatus.Allocated, inv.Blocks.Single(b => b.BlockId == block.BlockId).Status);
        Assert.Single(inv.Allocations);
    }

    [Fact]
    public void AllocateBlock_WhenBlockNotFound_Throws()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);
        var op = Operator.Create("MTN", "LIC-002");

        Assert.Throws<InvalidOperationException>(() =>
            inv.AllocateBlock(Guid.NewGuid(), op.OperatorId, DateTime.UtcNow));
    }

    [Fact]
    public void AllocateBlock_WhenAlreadyAllocated_Throws()
    {
        var inv = NumberingInventory.Create(NumberType.Msisdn);
        var block = inv.AddBlock(NumberRange.Create(60000000, 60009999));

        var op = Operator.Create("Econet", "LIC-003");
        var now = DateTime.UtcNow;

        inv.AllocateBlock(block.BlockId, op.OperatorId, now);

        Assert.Throws<InvalidOperationException>(() =>
            inv.AllocateBlock(block.BlockId, op.OperatorId, now.AddMinutes(1)));
    }
}
