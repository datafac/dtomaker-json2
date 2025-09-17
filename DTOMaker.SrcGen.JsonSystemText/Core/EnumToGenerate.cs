using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

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

    public readonly record struct MarkedInterface
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        public readonly string Fullname;
        public readonly EquatableArray<string> Values;
        public readonly string GeneratedNamespace;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(Fullname);

        public MarkedInterface(InterfaceDeclarationSyntax syntax, string fullname, List<string> values, string generatedNamespace,
            ImmutableArray<SyntaxDiagnostic> syntaxErrors)
        {
            Syntax = syntax;
            Fullname = fullname;
            Values = new(values.ToArray());
            GeneratedNamespace = generatedNamespace;
            SyntaxErrors = syntaxErrors;
        }
    }

}