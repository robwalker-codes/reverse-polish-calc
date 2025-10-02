using RpnCalc.Application.Abstractions;

namespace RpnCalc.Application.Commands;

public sealed record GetMemoryQuery(string SessionId);

public sealed class GetMemoryQueryHandler
{
    private readonly IMemoryStore _memoryStore;

    public GetMemoryQueryHandler(IMemoryStore memoryStore)
    {
        _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
    }

    public decimal Handle(GetMemoryQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.SessionId))
        {
            throw new ArgumentException("Session id is required.", nameof(query));
        }

        return _memoryStore.GetOrCreate(query.SessionId).Value;
    }
}
