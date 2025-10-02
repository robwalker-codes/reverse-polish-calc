using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Api.Contracts;

public sealed record EvaluateSettings(int? Precision, MidpointRounding? Rounding)
{
    public CalcSettings ToCalcSettings()
    {
        Precision precision = new(Precision ?? CalcSettings.Default.Precision.Digits);
        MidpointRounding rounding = Rounding ?? CalcSettings.Default.Rounding;
        return new(precision, rounding);
    }
}

public sealed record EvaluateResponse(string Result, ExpressionMode Mode, IReadOnlyList<string> Rpn, IReadOnlyList<string> Trace)
{
    public static EvaluateResponse From(EvaluationResult result, ExpressionMode mode)
    {
        List<string> rpn = result.RpnTokens.Select(token => token.Text).ToList();
        string formatted = result.Value.ToString(CultureInfo.InvariantCulture);
        return new(formatted, mode, rpn, result.Trace);
    }
}
