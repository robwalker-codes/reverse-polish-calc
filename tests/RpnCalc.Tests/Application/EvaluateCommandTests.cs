using FluentAssertions;
using RpnCalc.Application.Commands;
using RpnCalc.Application.ValueObjects;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Application
{
    public sealed class EvaluateCommandTests
    {
        [Theory]
        [InlineData("1+2", 3)]
        [InlineData("5-3", 2)]
        [InlineData("4*6", 24)]
        [InlineData("20/5", 4)]
        [InlineData("2^3", 8)]
        public void Handle_ShouldEvaluateSingleBinaryOperation(string expression, decimal expected)
        {
            EvaluateExpressionCommandHandler handler = CreateHandler();
            EvaluateExpressionCommand command = new(expression, ExpressionMode.Infix, false, null);

            EvaluationResult result = handler.Handle(command);

            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("(1+2)*3", 9)]
        [InlineData("4*(6-1)", 20)]
        [InlineData("(2+3)^(1+1)", 25)]
        public void Handle_ShouldRespectParenthesesGrouping(string expression, decimal expected)
        {
            EvaluateExpressionCommandHandler handler = CreateHandler();
            EvaluateExpressionCommand command = new(expression, ExpressionMode.Infix, false, null);

            EvaluationResult result = handler.Handle(command);

            result.Value.Should().Be(expected);
        }

        [Theory]
        [InlineData("2+3*4", 14)]
        [InlineData("10-6/3", 8)]
        [InlineData("8/4+6", 8)]
        [InlineData("5*3-4", 11)]
        public void Handle_ShouldApplyOperatorPrecedenceAcrossMultipleOperands(string expression, decimal expected)
        {
            EvaluateExpressionCommandHandler handler = CreateHandler();
            EvaluateExpressionCommand command = new(expression, ExpressionMode.Infix, false, null);

            EvaluationResult result = handler.Handle(command);

            result.Value.Should().Be(expected);
        }

        private static EvaluateExpressionCommandHandler CreateHandler()
        {
            return new EvaluateExpressionCommandHandler(new(), new(), new());
        }
    }
}
