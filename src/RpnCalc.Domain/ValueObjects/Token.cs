using System.Globalization;

namespace RpnCalc.Domain.ValueObjects;

public enum TokenType
{
    Number,
    Operator,
    Parenthesis
}

public abstract record Token(TokenType Type, string Text)
{
    public override string ToString() => Text;
}

public sealed record NumberLiteral(decimal Value, string Literal) : Token(TokenType.Number, Literal)
{
    public static NumberLiteral From(decimal value, int precision)
    {
        string literal = value.ToString($"F{precision}", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
        return new NumberLiteral(value, literal);
    }
}

public enum OperatorFixity
{
    Unary,
    Binary
}

public enum Associativity
{
    Left,
    Right
}

public sealed record Operator(
    string Symbol,
    int Precedence,
    Associativity Associativity,
    OperatorFixity Fixity,
    Func<decimal, decimal, decimal> Binary,
    Func<decimal, decimal> Unary) : Token(TokenType.Operator, Symbol)
{
    public decimal ApplyBinary(decimal left, decimal right) => Binary(left, right);

    public decimal ApplyUnary(decimal value) => Unary(value);
}

public sealed record Parenthesis(string Symbol, bool IsOpening) : Token(TokenType.Parenthesis, Symbol);
