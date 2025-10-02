using FluentAssertions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.Abstractions;
using RpnCalc.Domain.Memory;
using Xunit;

namespace RpnCalc.Tests.Application;

public sealed class MemoryCommandTests
{
    [Fact]
    public void ApplyMemory_ShouldStoreValue()
    {
        FakeMemoryStore store = new FakeMemoryStore();
        ApplyMemoryCommandHandler handler = new ApplyMemoryCommandHandler(store);
        ApplyMemoryCommand command = new ApplyMemoryCommand("session", MemoryCommandType.Store, 5m);

        decimal result = handler.Handle(command);

        result.Should().Be(5m);
        store.LastSaved.Should().Be(5m);
    }

    private sealed class FakeMemoryStore : IMemoryStore
    {
        public decimal? LastSaved { get; private set; }

        public void Clear(string sessionId)
        {
            LastSaved = 0m;
        }

        public MemoryRegister GetOrCreate(string sessionId)
        {
            return new MemoryRegister(LastSaved ?? 0m);
        }

        public void Save(string sessionId, MemoryRegister register)
        {
            LastSaved = register.Value;
        }
    }
}
