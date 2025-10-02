using System.Globalization;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Domain.Services;

public sealed class InfixTokenizer
{
    public IReadOnlyList<Token> Tokenize(InfixExpression expression)
    {
        List<Token> builder = new List<Token>();
        ReadOnlySpan<char> span = expression.Expression.AsSpan();
        int index = 0;
        while (index < span.Length)
        {
            if (char.IsWhiteSpace(span[index]))
            {
                index++;
                continue;
            }

            if (TryReadNumber(span, ref index, builder))
            {
                continue;
            }

            if (TryReadParenthesis(span, ref index, builder))
            {
                continue;
            }

            if (TryReadOperator(span, ref index, builder))
            {
                continue;
            }

            throw new TokenizationException($"Unexpected character '{span[index]}' at position {index}.");
        }

        EnsureBalance(builder);
        return builder;
    }

    private static bool TryReadNumber(ReadOnlySpan<char> span, ref int index, ICollection<Token> target)
    {
        if (!IsNumberStart(span[index]))
        {
            return false;
        }

        int start = index;
        index++;
        while (index < span.Length && (char.IsDigit(span[index]) || span[index] == '.'))
        {
            index++;
        }

        string literal = span[start..index].ToString();
        if (!decimal.TryParse(literal, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
        {
            throw new TokenizationException($"Invalid numeric literal '{literal}'.");
        }

        target.Add(new NumberLiteral(value, literal));
        return true;
    }

    private static bool TryReadParenthesis(ReadOnlySpan<char> span, ref int index, ICollection<Token> target)
    {
        char ch = span[index];
        if (ch != '(' && ch != ')')
        {
            return false;
        }

        target.Add(new Parenthesis(ch.ToString(), ch == '('));
        index++;
        return true;
    }

    private static bool TryReadOperator(ReadOnlySpan<char> span, ref int index, IList<Token> target)
    {
        string symbol = span[index].ToString();
        if (!OperatorCatalog.IsOperator(span[index]))
        {
            return false;
        }

        bool isUnary = IsUnary(target);
        target.Add(isUnary ? OperatorCatalog.GetUnary(symbol) : OperatorCatalog.GetBinary(symbol));
        index++;
        return true;
    }

    private static bool IsUnary(IReadOnlyList<Token> tokens)
    {
        if (tokens.Count == 0)
        {
            return true;
        }

        Token previous = tokens[^1];
        if (previous is Parenthesis paren && paren.IsOpening)
        {
            return true;
        }

        return previous is Operator && previous is not NumberLiteral;
    }

    private static bool IsNumberStart(char ch) => char.IsDigit(ch) || ch == '.';

    private static void EnsureBalance(IReadOnlyCollection<Token> tokens)
    {
        int depth = 0;
        foreach (Token token in tokens)
        {
            depth = UpdateDepth(token, depth);
            if (depth < 0)
            {
                throw new TokenizationException("Closing parenthesis without matching opening parenthesis.");
            }
        }

        if (depth != 0)
        {
            throw new TokenizationException("Unbalanced parentheses detected.");
        }
    }

    private static int UpdateDepth(Token token, int depth)
    {
        if (token is not Parenthesis paren)
        {
            return depth;
        }

        return paren.IsOpening ? depth + 1 : depth - 1;
    }
}
