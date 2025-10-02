namespace RpnCalc.Domain.ValueObjects;

public sealed record Precision
{
    public Precision(int digits)
    {
        if (digits is < 1 or > 28)
        {
            throw new ArgumentOutOfRangeException(nameof(digits), "Precision must be between 1 and 28.");
        }

        Digits = digits;
    }

    public int Digits { get; }
}

public sealed record CalcSettings(Precision Precision, MidpointRounding Rounding)
{
    public static CalcSettings Default { get; } = new(new Precision(15), MidpointRounding.ToEven);
}

public sealed record RpnExpression
{
    public RpnExpression(IReadOnlyList<Token> tokens)
    {
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    public IReadOnlyList<Token> Tokens { get; }
}

public sealed record InfixExpression
{
    public InfixExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required", nameof(expression));
        }

        Expression = expression;
    }

    public string Expression { get; }
}

public sealed record EvaluationResult
{
    public EvaluationResult(decimal value, IReadOnlyList<Token> rpnTokens, IReadOnlyList<string> trace)
    {
        Value = value;
        RpnTokens = rpnTokens;
        Trace = trace;
    }

    public decimal Value { get; }

    public IReadOnlyList<Token> RpnTokens { get; }

    public IReadOnlyList<string> Trace { get; }
}
