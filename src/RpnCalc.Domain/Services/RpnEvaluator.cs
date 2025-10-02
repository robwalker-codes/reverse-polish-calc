using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Domain.Services;

public sealed class RpnEvaluator
{
    public EvaluationResult Evaluate(RpnExpression expression, CalcSettings settings, bool includeTrace)
    {
        Stack<decimal> stack = new Stack<decimal>();
        List<string> trace = new List<string>();
        foreach (Token token in expression.Tokens)
        {
            EvaluateToken(token, stack, settings, trace, includeTrace);
        }

        if (stack.Count != 1)
        {
            throw new EvaluationException("Expression evaluation ended with unexpected stack state.");
        }

        decimal value = Round(stack.Pop(), settings);
        return new EvaluationResult(value, expression.Tokens, trace);
    }

    private static void EvaluateToken(Token token, Stack<decimal> stack, CalcSettings settings, ICollection<string> trace, bool includeTrace)
    {
        if (token is NumberLiteral literal)
        {
            PushNumber(literal.Value, stack, settings, trace, includeTrace);
            return;
        }

        ApplyOperator((Operator)token, stack, settings, trace, includeTrace);
    }

    private static void PushNumber(decimal value, Stack<decimal> stack, CalcSettings settings, ICollection<string> trace, bool includeTrace)
    {
        decimal rounded = Round(value, settings);
        stack.Push(rounded);
        if (includeTrace)
        {
            trace.Add($"push {rounded}");
        }
    }

    private static void ApplyOperator(Operator op, Stack<decimal> stack, CalcSettings settings, ICollection<string> trace, bool includeTrace)
    {
        if (op.Fixity == OperatorFixity.Unary)
        {
            ApplyUnary(op, stack, settings, trace, includeTrace);
            return;
        }

        ApplyBinary(op, stack, settings, trace, includeTrace);
    }

    private static void ApplyUnary(Operator op, Stack<decimal> stack, CalcSettings settings, ICollection<string> trace, bool includeTrace)
    {
        EnsureStack(stack, 1, op.Symbol);
        decimal value = stack.Pop();
        decimal result = Round(op.ApplyUnary(value), settings);
        stack.Push(result);
        if (includeTrace)
        {
            trace.Add($"apply {op.Symbol} -> {result}");
        }
    }

    private static void ApplyBinary(Operator op, Stack<decimal> stack, CalcSettings settings, ICollection<string> trace, bool includeTrace)
    {
        EnsureStack(stack, 2, op.Symbol);
        decimal right = stack.Pop();
        decimal left = stack.Pop();
        decimal result = Round(op.ApplyBinary(left, right), settings);
        stack.Push(result);
        if (includeTrace)
        {
            trace.Add($"apply {op.Symbol} -> {result}");
        }
    }

    private static void EnsureStack(Stack<decimal> stack, int expected, string op)
    {
        if (stack.Count < expected)
        {
            throw new EvaluationException($"Operator '{op}' requires {expected} operand(s).");
        }
    }

    private static decimal Round(decimal value, CalcSettings settings)
    {
        return Math.Round(value, settings.Precision.Digits, settings.Rounding);
    }
}
