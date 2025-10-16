using System;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedEntity
    {
        public readonly string NameSpace;
        public readonly string FullName;
        public readonly string IntfName;
        public readonly int EntityId;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I", StringComparison.Ordinal);

        public ParsedEntity(string nameSpace, string fullname, int entityId)
        {
            EntityId = entityId;
            NameSpace = nameSpace;
            FullName = fullname;

            // derived properties
            IntfName = fullname.Split('.').Last();
        }
    }
}
