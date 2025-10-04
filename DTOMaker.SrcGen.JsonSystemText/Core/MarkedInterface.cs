using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct MarkedInterface
    {
        public readonly InterfaceDeclarationSyntax Syntax;
        public readonly string NameSpace;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;
        public readonly ImmutableArray<SyntaxDiagnostic> SyntaxErrors;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I");

        public MarkedInterface(InterfaceDeclarationSyntax syntax, string fullname, int entityId, List<string> values, string nameSpace,
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

    public sealed record ModelMember
    {
        public string PropName { get; init; } = string.Empty;
    }
    public sealed record ModelEntity
    {
        public string NameSpace { get; init; } = string.Empty;
        public string IntfName { get; init; } = string.Empty;
        public int EntityId { get; init; }
        public EquatableArray<ModelMember> Members { get; init; } = new();
    }
    public sealed record ModelMetadata
    {
        public EquatableArray<ModelEntity> Entities { get; init; } = new();
    }
}

// adding this fixes CS0518 errors
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}