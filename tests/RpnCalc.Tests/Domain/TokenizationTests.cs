using FluentAssertions;
using RpnCalc.Domain.Exceptions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain;

public sealed class TokenizationTests
{
    [Fact]
    public void Tokenize_ShouldRecognizeUnaryMinus()
    {
        InfixTokenizer tokenizer = new InfixTokenizer();
        IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("(-3.5+2) * 4^2"));

        tokens.Should().ContainSingle(t => t.Text == "u-");
    }

    [Fact]
    public void Tokenize_ShouldThrowOnInvalidCharacter()
    {
        InfixTokenizer tokenizer = new InfixTokenizer();

        Action act = () => tokenizer.Tokenize(new InfixExpression("2 & 3"));

        act.Should().Throw<TokenizationException>()
            .WithMessage("Unexpected character '&' at position 2.*");
    }

    [Theory]
    [InlineData("(1 + 2")]
    [InlineData("1 + 2)")]
    public void Tokenize_ShouldRejectUnbalancedParentheses(string expression)
    {
        InfixTokenizer tokenizer = new InfixTokenizer();

        Action act = () => tokenizer.Tokenize(new InfixExpression(expression));

        act.Should().Throw<TokenizationException>();
    }

    [Fact]
    public void Tokenize_ShouldParseDecimalNumbersWithPrecision()
    {
        InfixTokenizer tokenizer = new InfixTokenizer();

        IReadOnlyList<Token> tokens = tokenizer.Tokenize(new InfixExpression("12.345 + .5"));

        tokens.Should().ContainSingle(t => t is NumberLiteral literal && literal.Value == 12.345m);
        tokens.Should().ContainSingle(t => t is NumberLiteral literal && literal.Value == 0.5m);
    }
}
