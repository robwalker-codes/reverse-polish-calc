using System.Linq;
using FluentAssertions;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain
{
    public sealed class TokenizationTests
    {
        [Fact]
        public void Tokenize_ShouldRecognizeUnaryMinus()
        {
            InfixTokenizer tokenizer = new();
            IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("(-3.5+2) * 4^2"));

            tokens.Should().ContainSingle(t => t.Text == "u-");
        }

        [Fact]
        public void Tokenize_ShouldThrowOnInvalidCharacter()
        {
            InfixTokenizer tokenizer = new();

            Action act = () => tokenizer.Tokenize(new InfixExpression("2 & 3"));

            act.Should().Throw<TokenizationException>()
                .WithMessage("Unexpected character '&' at position 2.*");
        }

        [Theory]
        [InlineData("(1 + 2")]
        [InlineData("1 + 2)")]
        public void Tokenize_ShouldRejectUnbalancedParentheses(string expression)
        {
            InfixTokenizer tokenizer = new();

            Action act = () => tokenizer.Tokenize(new InfixExpression(expression));

            act.Should().Throw<TokenizationException>();
        }

        [Fact]
        public void Tokenize_ShouldParseDecimalNumbersWithPrecision()
        {
            InfixTokenizer tokenizer = new();

            IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("12.345 + .5"));

            IEnumerable<NumberLiteral> numberLiterals = tokens.OfType<NumberLiteral>();
            numberLiterals.Should().ContainSingle(literal => literal.Value == 12.345m);
            numberLiterals.Should().ContainSingle(literal => literal.Value == 0.5m);
        }
    }
}
