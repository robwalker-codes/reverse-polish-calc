using System.Collections.Generic;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;

namespace RpnCalc.Api.Contracts;

public sealed record EvaluateRequest(
    string Expression,
    ExpressionMode Mode,
    bool ReturnTrace,
    EvaluateSettings? Settings,
    string? SessionId);

public sealed record KeyPressRequest(
    IReadOnlyList<string> Keys,
    ExpressionMode Mode,
    bool ReturnTrace,
    EvaluateSettings? Settings,
    string? SessionId);

public sealed record MemoryRequest(string SessionId, MemoryCommand Command, decimal? Value);

public sealed record ClearRequest(ClearScopeDto Scope);

public enum MemoryCommand
{
    MC,
    MR,
    MS,
    MPlus,
    MMinus
}

public enum ClearScopeDto
{
    CE,
    C,
    BACKSPACE
}
