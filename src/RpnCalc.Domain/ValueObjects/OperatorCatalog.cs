using System.Globalization;

namespace RpnCalc.Domain.ValueObjects;

public static class OperatorCatalog
{
    private static readonly IReadOnlyDictionary<string, Operator> Operators = new Dictionary<string, Operator>
    {
        ["+"] = new Operator("+", 1, Associativity.Left, OperatorFixity.Binary, (l, r) => l + r, v => v),
        ["-"] = new Operator("-", 1, Associativity.Left, OperatorFixity.Binary, (l, r) => l - r, v => -v),
        ["*"] = new Operator("*", 2, Associativity.Left, OperatorFixity.Binary, (l, r) => l * r, v => v),
        ["/"] = new Operator("/", 2, Associativity.Left, OperatorFixity.Binary, Divide, v => v),
        ["^"] = new Operator("^", 3, Associativity.Right, OperatorFixity.Binary, Power, v => v)
    };

    public static Operator GetBinary(string symbol)
    {
        return Operators.TryGetValue(symbol, out Operator? op)
            ? op
            : throw new ArgumentException($"Unsupported operator '{symbol}'.", nameof(symbol));
    }

    public static Operator GetUnary(string symbol)
    {
        return symbol switch
        {
            "+" => new Operator("u+", 4, Associativity.Right, OperatorFixity.Unary, (_, r) => r, v => v),
            "-" => new Operator("u-", 4, Associativity.Right, OperatorFixity.Unary, (_, r) => r, v => -v),
            _ => throw new ArgumentException($"Unsupported unary operator '{symbol}'.", nameof(symbol))
        };
    }

    public static bool IsOperator(char candidate) => Operators.ContainsKey(candidate.ToString());

    private static decimal Divide(decimal left, decimal right)
    {
        if (right == 0m)
        {
            throw new DivideByZeroException("Division by zero.");
        }

        return left / right;
    }

    private static decimal Power(decimal left, decimal right)
    {
        double result = Math.Pow((double)left, (double)right);
        return decimal.Parse(result.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }
}
