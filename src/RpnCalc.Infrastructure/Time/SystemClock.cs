using RpnCalc.Application.Abstractions;

namespace RpnCalc.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
