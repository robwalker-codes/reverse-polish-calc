using FluentAssertions;
using RpnCalc.Domain.Services;
using Xunit;

namespace RpnCalc.Tests.Domain
{
    public sealed class KeyStreamInterpreterTests
    {
        [Fact]
        public void InterpretInfix_ShouldAggregateDigitsAndOperators()
        {
            KeyStreamInterpreter interpreter = new();

            KeyStreamResult result = interpreter.Interpret(new[] { "1", "2", "+", "3", "=" }, CalculatorMode.Infix);

            result.Expression.Should().Be("12 + 3");
            result.ShouldEvaluate.Should().BeTrue();
        }

        [Fact]
        public void InterpretInfix_ShouldHandleBackspace()
        {
            KeyStreamInterpreter interpreter = new();

            KeyStreamResult result = interpreter.Interpret(new[] { "1", "2", "BACKSPACE", "+", "4" }, CalculatorMode.Infix);

            result.Expression.Should().Be("1 + 4");
            result.ShouldEvaluate.Should().BeFalse();
        }

        [Fact]
        public void InterpretRpn_ShouldRespectControlKeys()
        {
            KeyStreamInterpreter interpreter = new();

            KeyStreamResult result = interpreter.Interpret(new[] { "2", "3", "+", "CE", "4", "5", "*", "=" }, CalculatorMode.Rpn);

            result.Expression.Should().Be("4 5 *");
            result.ShouldEvaluate.Should().BeTrue();
        }

        [Fact]
        public void InterpretRpn_CCommandShouldResetEvaluation()
        {
            KeyStreamInterpreter interpreter = new();

            KeyStreamResult result = interpreter.Interpret(new[] { "2", "=", "C", "3" }, CalculatorMode.Rpn);

            result.Expression.Should().Be("3");
            result.ShouldEvaluate.Should().BeFalse();
        }
    }
}
