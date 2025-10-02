namespace RpnCalc.Application.Commands;

public enum ClearScope
{
    Entry,
    All,
    Backspace
}

public sealed record ClearCommand(ClearScope Scope);

public sealed class ClearCommandHandler
{
    public string Handle(ClearCommand command)
    {
        return command.Scope switch
        {
            ClearScope.Entry => string.Empty,
            ClearScope.All => string.Empty,
            ClearScope.Backspace => string.Empty,
            _ => string.Empty
        };
    }
}
