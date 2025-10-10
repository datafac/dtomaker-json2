using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedEntity
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        public readonly string NameSpace;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I");

        public ParsedEntity(InterfaceDeclarationSyntax syntax, string fullname, int entityId, List<string> values, string nameSpace,
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
    public readonly record struct ParsedMember
    {
        public readonly PropertyDeclarationSyntax Syntax;
        public readonly string FullName;
        public readonly string PropNameqqq;
        public readonly int Sequence;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(PropNameqqq);

        public ParsedMember(PropertyDeclarationSyntax syntax, string fullname, int sequence,
            ImmutableArray<SyntaxDiagnostic> syntaxErrors)
        {
            Syntax = syntax;
            FullName = fullname;
            Sequence = sequence;
            SyntaxErrors = syntaxErrors;

            // derived properties
            string propName = fullname.Split('.').Last();
            PropNameqqq = propName;
        }
    }
}
