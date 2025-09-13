using System;
using Xunit;

using NetEscapades.EnumGenerators;
using Shouldly;

namespace DTOMaker.SrcGen.JsonSystemText.IntTests;

[EnumExtensions]
[Flags]
public enum Colour
{
    Red = 1,
    Blue = 2,
    Green = 4,
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {

    }

    [Theory]
    [InlineData(Colour.Red)]
    [InlineData(Colour.Green)]
    [InlineData(Colour.Green | Colour.Blue)]
    [InlineData((Colour)15)]
    [InlineData((Colour)0)]
    public void FastToStringIsSameAsToString(Colour value)
    {
        var expected = value.ToString();
        var actual = value.ToStringFast();

        actual.ShouldBe(expected);
    }
}