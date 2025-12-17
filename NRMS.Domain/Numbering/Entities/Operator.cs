namespace NRMS.Domain.Numbering.Entities;

public sealed class Operator
{
    public Guid OperatorId { get; }
    public string Name { get; }
    public string? LicenseNumber { get; }

    public Operator(Guid operatorId, string name, string? licenseNumber = null)
    {
        if (operatorId == Guid.Empty) throw new ArgumentException("OperatorId is required.", nameof(operatorId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));

        OperatorId = operatorId;
        Name = name.Trim();
        LicenseNumber = string.IsNullOrWhiteSpace(licenseNumber) ? null : licenseNumber.Trim();
    }

    public static Operator Create(string name, string? licenseNumber = null)
        => new(Guid.NewGuid(), name, licenseNumber);
}
