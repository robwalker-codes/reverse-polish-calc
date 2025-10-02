namespace RpnCalc.Domain.Memory;

public sealed record MemoryRegister(decimal Value)
{
    public static MemoryRegister Empty { get; } = new(0m);

    public MemoryRegister Add(decimal delta) => new(Value + delta);

    public MemoryRegister Replace(decimal value) => new(value);

    public MemoryRegister Clear() => Empty;
}
