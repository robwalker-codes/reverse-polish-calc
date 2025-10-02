using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Domain.Services;

public sealed class InfixToRpnConverter
{
    public RpnExpression Convert(IReadOnlyList<Token> tokens)
    {
        List<Token> output = new();
        Stack<Token> operators = new();
        foreach (Token token in tokens)
        {
            HandleToken(token, output, operators);
        }

        DrainOperators(output, operators);
        return new RpnExpression(output);
    }

    private static void HandleToken(Token token, ICollection<Token> output, Stack<Token> operators)
    {
        if (token is NumberLiteral number)
        {
            output.Add(number);
            return;
        }

        if (token is Operator op)
        {
            PushOperator(op, output, operators);
            return;
        }

        HandleParenthesis((Parenthesis)token, output, operators);
    }

    private static void PushOperator(Operator current, ICollection<Token> output, Stack<Token> operators)
    {
        while (operators.TryPeek(out Token? top) && top is Operator topOperator && ShouldPop(topOperator, current))
        {
            output.Add(operators.Pop());
        }

        operators.Push(current);
    }

    private static bool ShouldPop(Operator top, Operator current)
    {
        if (top.Fixity == OperatorFixity.Unary)
        {
            return true;
        }

        if (current.Associativity == Associativity.Left)
        {
            return top.Precedence >= current.Precedence;
        }

        return top.Precedence > current.Precedence;
    }

    private static void HandleParenthesis(Parenthesis paren, ICollection<Token> output, Stack<Token> operators)
    {
        if (paren.IsOpening)
        {
            operators.Push(paren);
            return;
        }

        PopUntilOpening(output, operators);
    }

    private static void PopUntilOpening(ICollection<Token> output, Stack<Token> operators)
    {
        while (operators.Count > 0)
        {
            Token token = operators.Pop();
            if (token is Parenthesis opening)
            {
                if (!opening.IsOpening)
                {
                    throw new ConversionException("Mismatched parentheses detected.");
                }

                return;
            }

            output.Add(token);
        }

        throw new ConversionException("Closing parenthesis without matching opening parenthesis.");
    }

    private static void DrainOperators(ICollection<Token> output, Stack<Token> operators)
    {
        while (operators.Count > 0)
        {
            Token token = operators.Pop();
            if (token is Parenthesis)
            {
                throw new ConversionException("Unbalanced parentheses detected.");
            }

            output.Add(token);
        }
    }
}
