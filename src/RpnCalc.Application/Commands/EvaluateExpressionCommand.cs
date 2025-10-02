using System.Globalization;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;

namespace RpnCalc.Application.Commands;

public sealed record EvaluateExpressionCommand(
    string Expression,
    ExpressionMode Mode,
    bool ReturnTrace,
    CalcSettings? Settings);

public sealed class EvaluateExpressionCommandHandler
{
    private readonly InfixTokenizer _tokenizer;
    private readonly InfixToRpnConverter _converter;
    private readonly RpnEvaluator _evaluator;

    public EvaluateExpressionCommandHandler(InfixTokenizer tokenizer, InfixToRpnConverter converter, RpnEvaluator evaluator)
    {
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public EvaluationResult Handle(EvaluateExpressionCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Expression))
        {
            throw new ArgumentException("Expression is required.", nameof(command));
        }

        var settings = command.Settings ?? CalcSettings.Default;
        var rpn = CreateRpn(command, settings);
        return _evaluator.Evaluate(rpn, settings, command.ReturnTrace);
    }

    private RpnExpression CreateRpn(EvaluateExpressionCommand command, CalcSettings settings)
    {
        return command.Mode == ExpressionMode.Infix
            ? ConvertFromInfix(command.Expression, settings)
            : ParseRpn(command.Expression);
    }

    private RpnExpression ConvertFromInfix(string expression, CalcSettings settings)
    {
        var tokens = _tokenizer.Tokenize(new InfixExpression(expression));
        return _converter.Convert(tokens);
    }

    private static RpnExpression ParseRpn(string expression)
    {
        var tokens = new List<Token>();
        var span = expression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in span)
        {
            tokens.Add(ParseToken(part));
        }

        return new RpnExpression(tokens);
    }

    private static Token ParseToken(string symbol)
    {
        if (decimal.TryParse(symbol, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            return new NumberLiteral(value, symbol);
        }

        return symbol switch
        {
            "u-" or "neg" => OperatorCatalog.GetUnary("-"),
            "u+" or "pos" => OperatorCatalog.GetUnary("+"),
            _ => OperatorCatalog.GetBinary(symbol)
        };
    }
}
