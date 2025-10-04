using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct MarkedEntity
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        public readonly string NameSpace;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I");

        public MarkedEntity(InterfaceDeclarationSyntax syntax, string fullname, int entityId, List<string> values, string nameSpace,
            ImmutableArray<SyntaxDiagnostic> syntaxErrors)
        {
            Syntax = syntax;
            EntityId = entityId;
            Values = new(values.ToArray());
            NameSpace = nameSpace;
            SyntaxErrors = syntaxErrors;

            // derived properties
            string intfName = fullname.Split('.').Last();
            IntfName = intfName;
        }
    }
}
