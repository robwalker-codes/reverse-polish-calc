using RpnCalc.Domain.Memory;

namespace RpnCalc.Application.Abstractions;

public interface IMemoryStore
{
    MemoryRegister GetOrCreate(string sessionId);

    void Save(string sessionId, MemoryRegister register);

    void Clear(string sessionId);
}
