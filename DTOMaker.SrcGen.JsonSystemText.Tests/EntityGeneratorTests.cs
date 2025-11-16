using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.SrcGen.JsonSystemText.Tests
{
    public class EntityGeneratorTests
    {
        private static readonly string modelSource =
            """
            using DTOMaker.Models;
            using DTOMaker.Runtime;
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface IMyDTO : IEntityBase
                {
                    [Member(1)] int  Field1 { get; set; }
                    [Member(2)] int? Field2 { get; set; }
                }
                [Entity][Id(2)]
                public interface IDerived : IMyDTO
                {
                    [Member(1)] string  Field11 { get; set; }
                    [Member(2)] string? Field12 { get; set; }
                }
                [Entity][Id(3)]
                public interface IOther : IEntityBase
                {
                    [Member(1)] IMyDTO  Field31 { get; set; }
                    [Member(2)] IMyDTO? Field32 { get; set; }
                }
            }
            """;

        [Fact] public void EntitySrcGen_GeneratedSourcesLength() => modelSource.GenerateAndCheckLength(4);
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource0() => await Verifier.Verify(modelSource.GenerateAndGetOutput(0, "MyOrg.Models.JsonSystemText.EntityBase.g.cs"));
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource1() => await Verifier.Verify(modelSource.GenerateAndGetOutput(1, "MyOrg.Models.JsonSystemText.MyDTO.g.cs"));
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource2() => await Verifier.Verify(modelSource.GenerateAndGetOutput(2, "MyOrg.Models.JsonSystemText.Derived.g.cs"));
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource3() => await Verifier.Verify(modelSource.GenerateAndGetOutput(3, "MyOrg.Models.JsonSystemText.Other.g.cs"));
    }
}
