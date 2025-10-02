using System.Linq;
using FluentAssertions;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain
{
    public sealed class ConversionTests
    {
        [Fact]
        public void Convert_ShouldProduceExpectedRpn()
        {
            InfixTokenizer tokenizer = new();
            InfixToRpnConverter converter = new();
            IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("3 + 4 * 2 / (1 - 5) ^ 2 ^ 3"));

            RpnExpression rpn = converter.Convert(tokens);
            string[] sequence = rpn.Tokens.Select(t => t.Text).ToArray();

            sequence.Should().ContainInOrder("3", "4", "2", "*", "1", "5", "-", "2", "3", "^", "^", "/", "+");
        }

        [Fact]
        public void Convert_ShouldPreserveUnaryOperators()
        {
            InfixTokenizer tokenizer = new();
            InfixToRpnConverter converter = new();
            IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("-3 + 5"));

            RpnExpression rpn = converter.Convert(tokens);

            rpn.Tokens.Select(t => t.Text).Should().ContainInOrder("3", "u-", "5", "+");
        }

        [Fact]
        public void Convert_ShouldThrowOnUnmatchedClosingParenthesis()
        {
            InfixToRpnConverter converter = new();
            IReadOnlyList<Token> tokens = new Token[]
            {
                new NumberLiteral(2m, "2"),
                new Parenthesis(")", false)
            };

            Action act = () => converter.Convert(tokens);

            act.Should().Throw<ConversionException>()
                .WithMessage("Closing parenthesis without matching opening parenthesis.");
        }

        [Fact]
        public void Convert_ShouldThrowOnDanglingOpeningParenthesis()
        {
            InfixToRpnConverter converter = new();
            IReadOnlyList<Token> tokens = new Token[]
            {
                new NumberLiteral(2m, "2"),
                new Parenthesis("(", true)
            };

            Action act = () => converter.Convert(tokens);

            act.Should().Throw<ConversionException>()
                .WithMessage("Unbalanced parentheses detected.");
        }
    }
}
