using FluentAssertions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Services;
using Xunit;

namespace RpnCalc.Tests.Application;

public sealed class EvaluateCommandTests
{
    [Fact]
    public void Handle_ShouldEvaluateInfixExpression()
    {
        var handler = CreateHandler();
        var command = new EvaluateExpressionCommand("1+2", ExpressionMode.Infix, false, null);

        var result = handler.Handle(command);

        result.Value.Should().Be(3m);
    }

    private static EvaluateExpressionCommandHandler CreateHandler()
    {
        return new EvaluateExpressionCommandHandler(new InfixTokenizer(), new InfixToRpnConverter(), new RpnEvaluator());
    }
}
