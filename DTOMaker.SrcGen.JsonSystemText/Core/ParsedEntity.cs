using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedEntity
    {
        public readonly string NameSpace;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I");

        public ParsedEntity(string fullname, int entityId, List<string> values, string nameSpace)
        {
            EntityId = entityId;
            Values = new(values.ToArray());
            NameSpace = nameSpace;

            // derived properties
            IntfName = fullname.Split('.').Last();
        }
    }
}
