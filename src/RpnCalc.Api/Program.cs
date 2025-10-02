using System.Globalization;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using RpnCalc.Application.Abstractions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using RpnCalc.Infrastructure.Memory;
using RpnCalc.Infrastructure.Time;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PassthroughExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = RateLimitPartition.GetFixedWindowLimiter(
        _ => "global",
        _ => new FixedWindowRateLimiterOptions
        {
            AutoReplenishment = true,
            PermitLimit = 30,
            Window = TimeSpan.FromSeconds(60)
        });
});

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("spa", p =>
    {
        p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddSingleton<InfixTokenizer>();
builder.Services.AddSingleton<InfixToRpnConverter>();
builder.Services.AddSingleton<RpnEvaluator>();
builder.Services.AddSingleton<KeyStreamInterpreter>();
builder.Services.AddSingleton<IMemoryStore, InMemoryMemoryStore>();
builder.Services.AddSingleton<IClock, SystemClock>();

builder.Services.AddScoped<EvaluateExpressionCommandHandler>();
builder.Services.AddScoped<ApplyMemoryCommandHandler>();
builder.Services.AddScoped<ProcessKeysCommandHandler>();
builder.Services.AddScoped<GetMemoryQueryHandler>();
builder.Services.AddScoped<ClearCommandHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseCors("spa");
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var api = app.MapGroup("/api").RequireRateLimiting("global");
var v1 = api.MapGroup("/v1");

v1.MapPost("/calc/evaluate", async (EvaluateRequest request, EvaluateExpressionCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        var command = new EvaluateExpressionCommand(
            request.Expression,
            request.Mode,
            request.ReturnTrace,
            request.Settings?.ToCalcSettings());
        var result = handler.Handle(command);
        return Results.Ok(EvaluateResponse.From(result, request.Mode));
    });
}).WithOpenApi();

v1.MapPost("/calc/press", async (KeyPressRequest request, ProcessKeysCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        var command = new ProcessKeysCommand(request.Keys, request.Mode, request.ReturnTrace, request.Settings?.ToCalcSettings());
        var result = handler.Handle(command);
        return Results.Ok(EvaluateResponse.From(result, request.Mode));
    });
}).WithOpenApi();

v1.MapGet("/memory", async (string sessionId, GetMemoryQueryHandler handler) =>
{
    return await HandleAsync(() =>
    {
        var value = handler.Handle(new GetMemoryQuery(sessionId));
        return Results.Ok(new { sessionId, value });
    });
}).WithOpenApi();

v1.MapPost("/memory", async (MemoryRequest request, ApplyMemoryCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        var command = new ApplyMemoryCommand(request.SessionId, request.Command.ToCommand(), request.Value);
        var value = handler.Handle(command);
        return Results.Ok(new { request.SessionId, value });
    });
}).WithOpenApi();

v1.MapPost("/clear", async (ClearRequest request, ClearCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        var response = handler.Handle(new ClearCommand(request.Scope.ToScope()));
        return Results.Ok(new { cleared = response });
    });
}).WithOpenApi();

app.MapGet("/healthz", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/healthz/ready", (IClock clock) => Results.Ok(new { status = "ready", timestamp = clock.UtcNow }));

app.Run();

static Task<IResult> HandleAsync(Func<IResult> action)
{
    try
    {
        return Task.FromResult(action());
    }
    catch (CalculatorDomainException ex)
    {
        return Task.FromResult(Results.Problem(title: "Domain violation", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
    }
    catch (DivideByZeroException ex)
    {
        return Task.FromResult(Results.Problem(title: "Divide by zero", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest));
    }
    catch (Exception ex)
    {
        return Task.FromResult(Results.Problem(title: "Unexpected error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError));
    }
}

sealed class PassthroughExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(false);
    }
}

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

public sealed record EvaluateSettings(int? Precision, MidpointRounding? Rounding)
{
    public CalcSettings ToCalcSettings()
    {
        var precision = new Precision(Precision ?? CalcSettings.Default.Precision.Digits);
        var rounding = Rounding ?? CalcSettings.Default.Rounding;
        return new CalcSettings(precision, rounding);
    }
}

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

public sealed record EvaluateResponse(string Result, ExpressionMode Mode, IReadOnlyList<string> Rpn, IReadOnlyList<string> Trace)
{
    public static EvaluateResponse From(EvaluationResult result, ExpressionMode mode)
    {
        var rpn = result.RpnTokens.Select(t => t.Text).ToList();
        var formatted = result.Value.ToString(CultureInfo.InvariantCulture);
        return new EvaluateResponse(formatted, mode, rpn, result.Trace);
    }
}

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
