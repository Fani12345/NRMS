using NRMS.Application.Contracts.Numbering;
using NRMS.Application.Services;
using NRMS.Domain.Numbering;
using NRMS.Domain.Numbering.Entities;
using NRMS.Infrastructure;

namespace NRMS.Tests;

public sealed class NrmsNumberingServiceTests
{
    [Fact]
    public async Task AddNumberingBlock_WhenOverlapping_Throws()
    {
        var ops = new InMemoryOperatorRepository();
        var blocks = new InMemoryNumberingBlockRepository();
        var allocs = new InMemoryAllocationRepository();

        var svc = new NrmsNumberingService(ops, blocks, allocs);

        await svc.AddNumberingBlockAsync(new AddNumberingBlockCommand(
            NumberType: NumberType.Msisdn,
            Start: 60000000,
            End: 60009999,
            Notes: "Block 1"));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await svc.AddNumberingBlockAsync(new AddNumberingBlockCommand(
                NumberType: NumberType.Msisdn,
                Start: 60005000,
                End: 60015000,
                Notes: "Overlaps")));
    }

    [Fact]
    public async Task AllocateNumberingBlock_WhenValid_AllocatesAndCreatesAllocation()
    {
        var ops = new InMemoryOperatorRepository();
        var blocks = new InMemoryNumberingBlockRepository();
        var allocs = new InMemoryAllocationRepository();

        var svc = new NrmsNumberingService(ops, blocks, allocs);

        var opRes = await svc.CreateOperatorAsync(new CreateOperatorCommand("Vodacom", "LIC-001"));

        var blockRes = await svc.AddNumberingBlockAsync(new AddNumberingBlockCommand(
            NumberType: NumberType.Msisdn,
            Start: 60000000,
            End: 60009999,
            Notes: "Inventory"));

        var allocRes = await svc.AllocateNumberingBlockAsync(new AllocateNumberingBlockCommand(
            BlockId: blockRes.BlockId,
            OperatorId: opRes.OperatorId,
            ExpiresAtUtc: DateTime.UtcNow.AddDays(30),
            Notes: "Rollout"));

        var block = await blocks.GetAsync(blockRes.BlockId);
        Assert.NotNull(block);
        Assert.Equal(BlockStatus.Allocated, block!.Status);

        var allocation = await allocs.GetAsync(allocRes.AllocationId);
        Assert.NotNull(allocation);
        Assert.Equal(blockRes.BlockId, allocation!.BlockId);
        Assert.Equal(opRes.OperatorId, allocation.OperatorId);
    }

    [Fact]
    public async Task CreateOperator_WhenDuplicateName_Throws()
    {
        var ops = new InMemoryOperatorRepository();
        var blocks = new InMemoryNumberingBlockRepository();
        var allocs = new InMemoryAllocationRepository();

        var svc = new NrmsNumberingService(ops, blocks, allocs);

        await svc.CreateOperatorAsync(new CreateOperatorCommand("MTN", "LIC-002"));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await svc.CreateOperatorAsync(new CreateOperatorCommand("mtn", "LIC-XYZ")));
    }
}
