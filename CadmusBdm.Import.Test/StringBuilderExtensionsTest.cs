using System.Text;
using Xunit;

namespace CadmusBdm.Import.Test;

public sealed class StringBuilderExtensionsTest
{
    [Theory]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("a  ", "a")]
    [InlineData("  a", "a")]
    [InlineData("  a  ", "a")]
    [InlineData(" a  bc  ", "a bc")]
    [InlineData("a , b", "a, b")]
    [InlineData("a ( b ) c", "a (b) c")]
    public void StringBuilderExtensions_NormalizeWS(string text,
        string expected)
    {
        StringBuilder sb = new(text);

        sb.NormalizeWS(
            new[] { ',', '.', ':', ';', '?', '!', ')', ']', '}' },
            new[] { '(', '[', '{' });

        Assert.Equal(expected, sb.ToString());
    }
}