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
    public class EnumGeneratorSnapshotTests
    {
        // The source code to test
        private static readonly string sourceCode =
            """
            using NetEscapades.EnumGenerators;

            [EnumExtensions]
            public enum Colour
            {
                Red = 0,
                Blue = 1,
            }

            public enum OtherEnum
            {
                Red = 0,
                Blue = 1,
            }
            """;

        [Fact]
        public void EnumSrcGen00_GeneratedSourcesLengthShouldBe2()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(sourceCode, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Count(d => d.Id == "OK01").ShouldBe(1);
            generatorResult.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Info).ShouldBeGreaterThan(0);
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(2);
            generatorResult.GeneratedSources[0].HintName.ShouldBe("EnumExtensionsAttribute.g.cs");
            generatorResult.GeneratedSources[1].HintName.ShouldBe("EnumExtensions.Colour.g.cs");
        }

        [Fact]
        public async Task EnumSrcGen01_VerifyGeneratedSource0()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(sourceCode, LanguageVersion.LatestMajor);

            // custom generation checks
            var generated = generatorResult.GeneratedSources[0];
            string outputCode = string.Join(Environment.NewLine, generated.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }

        [Fact]
        public async Task EnumSrcGen01_VerifyGeneratedSource1()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(sourceCode, LanguageVersion.LatestMajor);

            // custom generation checks
            var generated = generatorResult.GeneratedSources[1];
            string outputCode = string.Join(Environment.NewLine, generated.SourceText.Lines.Select(tl => tl.ToString()));
            await Verifier.Verify(outputCode);
        }
    }
}
