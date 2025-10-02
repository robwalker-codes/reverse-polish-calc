using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using RpnCalc.Api.Contracts;
using RpnCalc.Api.Infrastructure;
using RpnCalc.Application.Abstractions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using RpnCalc.Infrastructure.Memory;
using RpnCalc.Infrastructure.Time;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration).WriteTo.Console());

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<PassthroughExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("global", limiterOptions =>
    {
        limiterOptions.AutoReplenishment = true;
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromSeconds(60);
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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

WebApplication app = builder.Build();
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

RouteGroupBuilder api = app.MapGroup("/api").RequireRateLimiting("global");
RouteGroupBuilder v1 = api.MapGroup("/v1");

v1.MapPost("/calc/evaluate", async (EvaluateRequest request, EvaluateExpressionCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        EvaluateExpressionCommand command = new(
            request.Expression,
            request.Mode,
            request.ReturnTrace,
            request.Settings?.ToCalcSettings());
        EvaluationResult result = handler.Handle(command);
        return Results.Ok(EvaluateResponse.From(result, request.Mode));
    });
});

v1.MapPost("/calc/press", async (KeyPressRequest request, ProcessKeysCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        ProcessKeysCommand command = new(request.Keys, request.Mode, request.ReturnTrace, request.Settings?.ToCalcSettings());
        EvaluationResult result = handler.Handle(command);
        return Results.Ok(EvaluateResponse.From(result, request.Mode));
    });
});

v1.MapGet("/memory", async (string sessionId, GetMemoryQueryHandler handler) =>
{
    return await HandleAsync(() =>
    {
        decimal value = handler.Handle(new(sessionId));
        return Results.Ok(new { sessionId, value });
    });
});

v1.MapPost("/memory", async (MemoryRequest request, ApplyMemoryCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        ApplyMemoryCommand command = new(request.SessionId, request.Command.ToCommand(), request.Value);
        decimal value = handler.Handle(command);
        return Results.Ok(new { request.SessionId, value });
    });
});

v1.MapPost("/clear", async (ClearRequest request, ClearCommandHandler handler) =>
{
    return await HandleAsync(() =>
    {
        string response = handler.Handle(new(request.Scope.ToScope()));
        return Results.Ok(new { cleared = response });
    });
});

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

