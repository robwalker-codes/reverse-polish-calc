using System.Collections.Concurrent;
using RpnCalc.Application.Abstractions;
using RpnCalc.Domain.Memory;

namespace RpnCalc.Infrastructure.Memory;

public sealed class InMemoryMemoryStore : IMemoryStore
{
    private readonly ConcurrentDictionary<string, MemoryRegister> _store = new();

    public MemoryRegister GetOrCreate(string sessionId)
    {
        return _store.GetOrAdd(sessionId, _ => MemoryRegister.Empty);
    }

    public void Save(string sessionId, MemoryRegister register)
    {
        _store.AddOrUpdate(sessionId, register, (_, _) => register);
    }

    public void Clear(string sessionId)
    {
        _store.TryRemove(sessionId, out _);
    }
}
