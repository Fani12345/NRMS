using NRMS.Application.Abstractions;

namespace NRMS.Infrastructure;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
