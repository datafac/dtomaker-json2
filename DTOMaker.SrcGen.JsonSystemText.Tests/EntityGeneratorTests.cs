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
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface IMyDTO
                {
                }
            }
            """;

        [Fact] public void EntitySrcGen_GeneratedSourcesLength() => modelSource.GenerateAndCheckLength(2);
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource0() => await Verifier.Verify(modelSource.GenerateAndGetOutput(0, "Metadata.Summary.g.cs"));
        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource1() => await Verifier.Verify(modelSource.GenerateAndGetOutput(1, "MyOrg.Models.JsonSystemText.MyDTO.g.cs"));

    }
}
