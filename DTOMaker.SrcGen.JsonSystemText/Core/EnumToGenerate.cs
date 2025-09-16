using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct EnumToGenerate
    {
        public readonly EnumDeclarationSyntax Syntax;
        public readonly string Name;
        public readonly EquatableArray<string> Values;
        public readonly string GeneratedClassName;
        public readonly string GeneratedNamespace;

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Values.Count > 0;

        public EnumToGenerate(EnumDeclarationSyntax syntax, string name, List<string> values, string generatedClassName, string generatedNamespace)
        {
            Syntax = syntax;
            Name = name;
            Values = new(values.ToArray());
            GeneratedClassName = generatedClassName;
            GeneratedNamespace = generatedNamespace;
        }
    }

}