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

        private static string GenerateAndGetOutput(int index, string expectedHintName)
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(modelSource, LanguageVersion.LatestMajor);
            var generated = generatorResult.GeneratedSources[index];
            generated.HintName.ShouldBe(expectedHintName);
            string outputCode = string.Join(Environment.NewLine, generated.SourceText.Lines.Select(tl => tl.ToString()));
            return outputCode;
        }

        [Fact]
        public void EntitySrcGen_GeneratedSourcesLength()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(modelSource, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Count(d => d.Id == "OK01").ShouldBe(1);
            generatorResult.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Info).ShouldBeGreaterThan(0);
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(1);
        }

        [Fact] public async Task EntitySrcGen_VerifyGeneratedSource0() => await Verifier.Verify(GenerateAndGetOutput(0, "Metadata.Summary.g.cs"));

    }
}
