using System.Text;

namespace RpnCalc.Domain.Services;

public enum CalculatorMode
{
    Infix,
    Rpn
}

public sealed record KeyStreamResult(string Expression, bool ShouldEvaluate);

public sealed class KeyStreamInterpreter
{
    public KeyStreamResult Interpret(IReadOnlyList<string> keys, CalculatorMode mode)
    {
        return mode switch
        {
            CalculatorMode.Infix => InterpretInfix(keys),
            CalculatorMode.Rpn => InterpretRpn(keys),
            _ => throw new ArgumentOutOfRangeException(nameof(mode))
        };
    }

    private static KeyStreamResult InterpretInfix(IReadOnlyList<string> keys)
    {
        var buffer = new StringBuilder();
        var evaluate = false;
        foreach (var key in keys)
        {
            if (HandleControlKey(key, buffer, ref evaluate))
            {
                continue;
            }

            AppendToken(buffer, key);
        }

        return new KeyStreamResult(buffer.ToString().Trim(), evaluate);
    }

    private static bool HandleControlKey(string key, StringBuilder buffer, ref bool evaluate)
    {
        return key switch
        {
            "CE" => ClearBuffer(buffer),
            "C" => ClearAndReset(buffer, ref evaluate),
            "BACKSPACE" => Backspace(buffer),
            "=" => SetEvaluate(ref evaluate),
            _ => false
        };
    }

    private static bool ClearBuffer(StringBuilder buffer)
    {
        buffer.Clear();
        return true;
    }

    private static bool ClearAndReset(StringBuilder buffer, ref bool evaluate)
    {
        buffer.Clear();
        evaluate = false;
        return true;
    }

    private static bool Backspace(StringBuilder buffer)
    {
        if (buffer.Length == 0)
        {
            return true;
        }

        buffer.Remove(buffer.Length - 1, 1);
        return true;
    }

    private static bool SetEvaluate(ref bool evaluate)
    {
        evaluate = true;
        return true;
    }

    private static void AppendToken(StringBuilder buffer, string key)
    {
        if (IsDigitKey(key))
        {
            buffer.Append(key);
            return;
        }

        if (buffer.Length > 0 && buffer[^1] != ' ')
        {
            buffer.Append(' ');
        }

        buffer.Append(key);
        buffer.Append(' ');
    }

    private static bool IsDigitKey(string key)
    {
        return key.Length == 1 && (char.IsDigit(key[0]) || key == ".");
    }

    private static KeyStreamResult InterpretRpn(IReadOnlyList<string> keys)
    {
        var buffer = new List<string>();
        var evaluate = false;
        foreach (var key in keys)
        {
            if (key == "CE")
            {
                buffer.Clear();
                continue;
            }

            if (key == "C")
            {
                buffer.Clear();
                evaluate = false;
                continue;
            }

            if (key == "BACKSPACE")
            {
                if (buffer.Count > 0)
                {
                    buffer.RemoveAt(buffer.Count - 1);
                }

                continue;
            }

            if (key == "=")
            {
                evaluate = true;
                continue;
            }

            buffer.Add(key);
        }

        return new KeyStreamResult(string.Join(' ', buffer), evaluate);
    }
}
