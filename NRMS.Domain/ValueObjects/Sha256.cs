using System.Text.RegularExpressions;
using NRMS.Domain.Exceptions;

namespace NRMS.Domain.ValueObjects;

public readonly record struct Sha256
{
    private static readonly Regex Hex64 = new("^[a-f0-9]{64}$", RegexOptions.Compiled);

    public string Value { get; }

    public Sha256(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("SHA-256 value is required.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!Hex64.IsMatch(normalized))
            throw new DomainException("SHA-256 value must be 64 hex characters.");

        Value = normalized;
    }

    public override string ToString() => Value;

    public static implicit operator string(Sha256 sha) => sha.Value;
    public static explicit operator Sha256(string value) => new(value);
}
