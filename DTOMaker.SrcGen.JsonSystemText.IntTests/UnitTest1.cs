using System;
using Xunit;
using Shouldly;
using DTOMaker.Models;
using DTOMaker.Runtime;

namespace DTOMaker.SrcGen.JsonSystemText.IntTests;

[Entity]
[Id(1)]
public interface IMyDTO : IEntityBase
{
    int Field1 { get; set; }
}

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        //IMyDTO dto = new MyDTO { Field1 = 42 };
        //dto.Field1.ShouldBe(42);
    }
}