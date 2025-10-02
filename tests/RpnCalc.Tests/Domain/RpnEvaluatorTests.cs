using System;
using FluentAssertions;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain
{
    public sealed class RpnEvaluatorTests
    {
        private readonly RpnEvaluator _evaluator = new();

        [Fact]
        public void Evaluate_ShouldHandleExponentiation()
        {
            RpnExpression expression = new(new Token[]
            {
                new NumberLiteral(2m, "2"),
                new NumberLiteral(3m, "3"),
                OperatorCatalog.GetBinary("^")
            });

            EvaluationResult result = _evaluator.Evaluate(expression, CalcSettings.Default, false);

            result.Value.Should().Be(8m);
        }

        [Fact]
        public void Evaluate_ShouldFailOnInsufficientOperands()
        {
            RpnExpression expression = new(new Token[] { OperatorCatalog.GetBinary("+") });

            Action action = () => _evaluator.Evaluate(expression, CalcSettings.Default, false);

            action.Should().Throw<EvaluationException>();
        }

        [Fact]
        public void Evaluate_ShouldApplyUnaryOperators()
        {
            RpnExpression expression = new(new Token[]
            {
                new NumberLiteral(3m, "3"),
                OperatorCatalog.GetUnary("-"),
                new NumberLiteral(2m, "2"),
                OperatorCatalog.GetBinary("+")
            });

            EvaluationResult result = _evaluator.Evaluate(expression, CalcSettings.Default, false);

            result.Value.Should().Be(-1m);
        }

        [Fact]
        public void Evaluate_ShouldRoundAccordingToSettings()
        {
            CalcSettings settings = new(new Precision(2), MidpointRounding.AwayFromZero);
            RpnExpression expression = new(new Token[]
            {
                new NumberLiteral(1.234m, "1.234"),
                new NumberLiteral(1.111m, "1.111"),
                OperatorCatalog.GetBinary("+")
            });

            EvaluationResult result = _evaluator.Evaluate(expression, settings, false);

            result.Value.Should().Be(2.34m);
        }

        [Fact]
        public void Evaluate_ShouldIncludeTraceWhenRequested()
        {
            RpnExpression expression = new(new Token[]
            {
                new NumberLiteral(2m, "2"),
                new NumberLiteral(3m, "3"),
                OperatorCatalog.GetBinary("+")
            });

            EvaluationResult result = _evaluator.Evaluate(expression, CalcSettings.Default, true);

            result.Trace.Should().ContainInOrder("push 2", "push 3", "apply + -> 5");
        }

        [Fact]
        public void Evaluate_ShouldPropagateDivideByZero()
        {
            RpnExpression expression = new(new Token[]
            {
                new NumberLiteral(5m, "5"),
                new NumberLiteral(0m, "0"),
                OperatorCatalog.GetBinary("/")
            });

            Action action = () => _evaluator.Evaluate(expression, CalcSettings.Default, false);

            action.Should().Throw<DivideByZeroException>();
        }
    }
}
