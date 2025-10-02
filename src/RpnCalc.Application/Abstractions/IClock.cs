namespace RpnCalc.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
