using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct MarkedInterface
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        //public readonly string Fullname;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;
        public readonly string NameSpace;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public readonly string IntfName;
        public readonly string ImplName;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I");

        public MarkedInterface(InterfaceDeclarationSyntax syntax, string fullname, int entityId, List<string> values, string nameSpace,
            ImmutableArray<SyntaxDiagnostic> syntaxErrors)
        {
            Syntax = syntax;
            //Fullname = fullname;
            EntityId = entityId;
            Values = new(values.ToArray());
            NameSpace = nameSpace;
            SyntaxErrors = syntaxErrors;

            // derived properties
            string intfName = fullname.Split('.').Last();
            IntfName = intfName;
            ImplName = IntfName.Substring(1);
        }
    }

}