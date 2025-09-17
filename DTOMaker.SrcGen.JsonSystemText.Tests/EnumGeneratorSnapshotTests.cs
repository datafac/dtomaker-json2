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

            namespace MyNamespace;

            [EnumExtensions(ExtensionClassName = "ColourExtensions")]
            public enum Colour
            {
                Red = 0,
                Blue = 1,
            }
            
            [EnumExtensions]
            public enum Gender
            {
                None = 0,
                Male = 1,
                Female = 2,
            }
            
            public enum OtherEnum
            {
                Red = 0,
                Blue = 1,
            }
            """;

        private static string GenerateAndGetOutput(int index, string expectedHintName)
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(sourceCode, LanguageVersion.LatestMajor);
            var generated = generatorResult.GeneratedSources[index];
            generated.HintName.ShouldBe(expectedHintName);
            string outputCode = string.Join(Environment.NewLine, generated.SourceText.Lines.Select(tl => tl.ToString()));
            return outputCode;
        }

        [Fact]
        public void EnumSrcGen_GeneratedSourcesLength()
        {
            var generatorResult = GeneratorTestHelper.RunSourceGenerator(sourceCode, LanguageVersion.LatestMajor);
            generatorResult.Exception.ShouldBeNull();
            generatorResult.Diagnostics.Count(d => d.Id == "OK01").ShouldBe(1);
            generatorResult.Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Info).ShouldBeGreaterThan(0);
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).ShouldBeEmpty();
            generatorResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ShouldBeEmpty();

            // custom generation checks
            generatorResult.GeneratedSources.Length.ShouldBe(5);
        }

        [Fact] public async Task EnumSrcGen_VerifyGeneratedSource0() => await Verifier.Verify(GenerateAndGetOutput(0, "EnumExtensionsAttribute.g.cs"));
        [Fact] public async Task EnumSrcGen_VerifyGeneratedSource1() => await Verifier.Verify(GenerateAndGetOutput(1, "EnumExtensions.MyNamespace.Colour.g.cs"));
        [Fact] public async Task EnumSrcGen_VerifyGeneratedSource2() => await Verifier.Verify(GenerateAndGetOutput(2, "EnumExtensions.MyNamespace.Gender.g.cs"));
        [Fact] public async Task EnumSrcGen_VerifyGeneratedSource3() => await Verifier.Verify(GenerateAndGetOutput(3, "EnumExtensions.Summary.g.cs"));
        [Fact] public async Task EnumSrcGen_VerifyGeneratedSource4() => await Verifier.Verify(GenerateAndGetOutput(4, "Metadata.Summary.g.cs"));

    }
}
