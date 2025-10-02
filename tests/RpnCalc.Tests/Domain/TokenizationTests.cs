using FluentAssertions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain;

public sealed class TokenizationTests
{
    [Fact]
    public void Tokenize_ShouldRecognizeUnaryMinus()
    {
        var tokenizer = new InfixTokenizer();
        var tokens = tokenizer.Tokenize(new InfixExpression("(-3.5+2) * 4^2"));

        tokens.Should().ContainSingle(t => t.Text == "u-");
    }
}
