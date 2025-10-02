using FluentAssertions;
using RpnCalc.Domain.Services;
using RpnCalc.Domain.ValueObjects;
using Xunit;

namespace RpnCalc.Tests.Domain;

public sealed class ConversionTests
{
    [Fact]
    public void Convert_ShouldProduceExpectedRpn()
    {
        var tokenizer = new InfixTokenizer();
        var converter = new InfixToRpnConverter();
        var tokens = tokenizer.Tokenize(new InfixExpression("3 + 4 * 2 / (1 - 5) ^ 2 ^ 3"));

        var rpn = converter.Convert(tokens);
        var sequence = rpn.Tokens.Select(t => t.Text).ToArray();

        sequence.Should().ContainInOrder("3", "4", "2", "*", "1", "5", "-", "2", "3", "^", "^", "/", "+");
    }
}
