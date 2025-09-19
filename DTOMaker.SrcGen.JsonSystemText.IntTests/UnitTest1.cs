using System;
using Xunit;
using Shouldly;
using DTOMaker.Models;

namespace DTOMaker.SrcGen.JsonSystemText.IntTests;

[Entity]
[Id(1)]
public interface IMyDTO
{
    int Field1 { get; set; }
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {

    }
}