using System;
using RpnCalc.Application.Commands;

namespace RpnCalc.Api.Contracts;

public static class MemoryCommandExtensions
{
    public static MemoryCommandType ToCommand(this MemoryCommand command)
    {
        return command switch
        {
            MemoryCommand.MC => MemoryCommandType.Clear,
            MemoryCommand.MR => MemoryCommandType.Recall,
            MemoryCommand.MS => MemoryCommandType.Store,
            MemoryCommand.MPlus => MemoryCommandType.Add,
            MemoryCommand.MMinus => MemoryCommandType.Subtract,
            _ => throw new ArgumentOutOfRangeException(nameof(command))
        };
    }
}

public static class ClearScopeExtensions
{
    public static ClearScope ToScope(this ClearScopeDto scope)
    {
        return scope switch
        {
            ClearScopeDto.CE => ClearScope.Entry,
            ClearScopeDto.C => ClearScope.All,
            ClearScopeDto.BACKSPACE => ClearScope.Backspace,
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };
    }
}
