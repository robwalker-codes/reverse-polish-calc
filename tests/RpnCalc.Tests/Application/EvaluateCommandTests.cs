using FluentAssertions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Application;

public sealed class EvaluateCommandTests
{
    [Fact]
    public void Handle_ShouldEvaluateInfixExpression()
    {
        EvaluateExpressionCommandHandler handler = CreateHandler();
        EvaluateExpressionCommand command = new EvaluateExpressionCommand("1+2", ExpressionMode.Infix, false, null);

        EvaluationResult result = handler.Handle(command);

        result.Value.Should().Be(3m);
    }

    private static EvaluateExpressionCommandHandler CreateHandler()
    {
        return new EvaluateExpressionCommandHandler(new InfixTokenizer(), new InfixToRpnConverter(), new RpnEvaluator());
    }
}
