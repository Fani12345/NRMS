namespace NRMS.Domain.Numbering.ValueObjects;

public readonly record struct NumberRange(long Start, long End)
{
    public int LengthInclusive => checked((int)(End - Start + 1));

    public static NumberRange Create(long start, long end)
    {
        if (start <= 0) throw new ArgumentOutOfRangeException(nameof(start), "Start must be positive.");
        if (end <= 0) throw new ArgumentOutOfRangeException(nameof(end), "End must be positive.");
        if (end < start) throw new ArgumentException("End must be >= Start.");
        return new NumberRange(start, end);
    }

    public bool Contains(long value) => value >= Start && value <= End;

    public bool Contains(NumberRange other) => other.Start >= Start && other.End <= End;

    public bool Overlaps(NumberRange other) => !(other.End < Start || other.Start > End);

    public override string ToString() => $"{Start}-{End}";
}
