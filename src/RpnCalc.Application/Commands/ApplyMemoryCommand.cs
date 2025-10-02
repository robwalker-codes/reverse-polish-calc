using RpnCalc.Application.Abstractions;
using RpnCalc.Domain.Memory;

namespace RpnCalc.Application.Commands;

public enum MemoryCommandType
{
    Clear,
    Recall,
    Store,
    Add,
    Subtract
}

public sealed record ApplyMemoryCommand(string SessionId, MemoryCommandType Command, decimal? Value);

public sealed class ApplyMemoryCommandHandler
{
    private readonly IMemoryStore _memoryStore;

    public ApplyMemoryCommandHandler(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
    }

    public decimal Handle(ApplyMemoryCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.SessionId))
        {
            throw new ArgumentException("Session id is required.", nameof(command));
        }

        MemoryRegister register = _memoryStore.GetOrCreate(command.SessionId);
        MemoryRegister result = Execute(command, register);
        Persist(command, result);
        return result.Value;
    }

    private static MemoryRegister Execute(ApplyMemoryCommand command, MemoryRegister register)
    {
        return command.Command switch
        {
            MemoryCommandType.Clear => register.Clear(),
            MemoryCommandType.Recall => register,
            MemoryCommandType.Store => register.Replace(command.Value ?? throw new ArgumentException("Value required", nameof(command))),
            MemoryCommandType.Add => register.Add(command.Value ?? throw new ArgumentException("Value required", nameof(command))),
            MemoryCommandType.Subtract => register.Add(-(command.Value ?? throw new ArgumentException("Value required", nameof(command)))),
            _ => register
        };
    }

    private void Persist(ApplyMemoryCommand command, MemoryRegister register)
    {
        if (command.Command == MemoryCommandType.Clear)
        {
            _memoryStore.Clear(command.SessionId);
            return;
        }

        _memoryStore.Save(command.SessionId, register);
    }
}
