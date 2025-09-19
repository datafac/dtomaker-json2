using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct MarkedInterface
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        public readonly string Fullname;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;
        public readonly string GeneratedNamespace;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(Fullname);

        public MarkedInterface(InterfaceDeclarationSyntax syntax, string fullname, int entityId, List<string> values, string generatedNamespace,
            ImmutableArray<SyntaxDiagnostic> syntaxErrors)
        {
            Syntax = syntax;
            Fullname = fullname;
            EntityId = entityId;
            Values = new(values.ToArray());
            GeneratedNamespace = generatedNamespace;
            SyntaxErrors = syntaxErrors;
        }
    }

}