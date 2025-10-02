using Microsoft.AspNetCore.Http;
using RpnCalc.Application.Abstractions;

namespace RpnCalc.Api.RateLimiting;

public sealed class FixedWindowRateLimiter
{
    private readonly object _sync = new();
    private readonly IClock _clock;
    private readonly TimeSpan _window;
    private readonly int _limit;
    private DateTimeOffset _windowStart;
    private int _count;

    public FixedWindowRateLimiter(IClock clock, TimeSpan window, int limit)
    {
        _clock = clock;
        _window = window;
        _limit = limit;
        _windowStart = _clock.UtcNow;
    }

    public bool TryAcquire()
    {
        lock (_sync)
        {
            ResetWindow();
            return IncrementWithinLimit();
        }
    }

    private void ResetWindow()
    {
        var now = _clock.UtcNow;
        if (now - _windowStart < _window)
        {
            return;
        }

        _windowStart = now;
        _count = 0;
    }

    private bool IncrementWithinLimit()
    {
        if (_count >= _limit)
        {
            return false;
        }

        _count++;
        return true;
    }
}

public sealed class EvaluateRateLimiterMiddleware
{
    private static readonly PathString EvaluatePath = new("/api/v1/calc/evaluate");
    private readonly RequestDelegate _next;
    private readonly FixedWindowRateLimiter _limiter;

    public EvaluateRateLimiterMiddleware(RequestDelegate next, FixedWindowRateLimiter limiter)
    {
        _next = next;
        _limiter = limiter;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (IsEvaluateRequest(context.Request.Path) && !_limiter.TryAcquire())
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return context.Response.WriteAsJsonAsync(new
            {
                title = "Rate limit exceeded",
                detail = "Too many requests. Please retry shortly."
            });
        }

        return _next(context);
    }

    private static bool IsEvaluateRequest(PathString path)
    {
        return path.Equals(EvaluatePath, StringComparison.OrdinalIgnoreCase);
    }
}
