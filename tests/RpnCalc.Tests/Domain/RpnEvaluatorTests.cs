using FluentAssertions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain;

public sealed class RpnEvaluatorTests
{
    private readonly RpnEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ShouldHandleExponentiation()
    {
        var expression = new RpnExpression(new Token[]
        {
            new NumberLiteral(2m, "2"),
            new NumberLiteral(3m, "3"),
            OperatorCatalog.GetBinary("^")
        });

        var result = _evaluator.Evaluate(expression, CalcSettings.Default, false);

        result.Value.Should().Be(8m);
    }

    [Fact]
    public void Evaluate_ShouldFailOnInsufficientOperands()
    {
        var expression = new RpnExpression(new Token[] { OperatorCatalog.GetBinary("+") });

        Action action = () => _evaluator.Evaluate(expression, CalcSettings.Default, false);

        action.Should().Throw<EvaluationException>();
    }
}
