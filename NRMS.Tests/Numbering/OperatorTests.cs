using NRMS.Domain.Numbering.Entities;

namespace NRMS.Tests.Numbering;

public sealed class OperatorTests
{
    [Fact]
    public void Create_WhenValid_SetsFields()
    {
        var op = Operator.Create("Vodacom", "LIC-001");

        Assert.NotEqual(Guid.Empty, op.OperatorId);
        Assert.Equal("Vodacom", op.Name);
        Assert.Equal("LIC-001", op.LicenseNumber);
    }

    [Fact]
    public void Constructor_WhenNameMissing_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Operator(Guid.NewGuid(), ""));
    }

    [Fact]
    public void Constructor_WhenIdEmpty_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Operator(Guid.Empty, "MTN"));
    }
}
