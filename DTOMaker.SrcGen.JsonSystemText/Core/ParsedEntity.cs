using System;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedEntity
    {
        public readonly string NameSpace;
        public readonly string FullName;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<string> Values;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I", StringComparison.Ordinal);

        public ParsedEntity(string fullname, int entityId, string[] values, string nameSpace)
        {
            EntityId = entityId;
            Values = new(values);
            NameSpace = nameSpace;
            FullName = fullname;

            // derived properties
            IntfName = fullname.Split('.').Last();
        }
    }

    public readonly record struct OutputEntity
    {
        public readonly string NameSpace;
        public readonly string FullName;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<OutputMember> Members;

        public OutputEntity(string nameSpace, string fullname, string intfName, int entityId, OutputMember[] members)
        {
            NameSpace = nameSpace;
            FullName = fullname;
            IntfName = intfName;
            EntityId = entityId;
            Members = new(members);
        }
    }
}
