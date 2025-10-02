namespace RpnCalc.Domain.Exceptions;

public abstract class CalculatorDomainException : Exception
{
    protected CalculatorDomainException(string message)
        : base(message)
    {
    }
}

public sealed class TokenizationException : CalculatorDomainException
{
    public TokenizationException(string message)
        : base(message)
    {
    }
}

public sealed class ConversionException : CalculatorDomainException
{
    public ConversionException(string message)
        : base(message)
    {
    }
}

public sealed class EvaluationException : CalculatorDomainException
{
    public EvaluationException(string message)
        : base(message)
    {
    }
}
