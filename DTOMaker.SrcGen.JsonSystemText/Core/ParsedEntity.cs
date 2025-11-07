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
        public readonly string? BaseFullName;

        public bool IsValid => !string.IsNullOrWhiteSpace(NameSpace) && !string.IsNullOrWhiteSpace(IntfName) && IntfName.StartsWith("I", StringComparison.Ordinal);

        public ParsedEntity(string fullname, int entityId, string? baseFullName)
        {
            EntityId = entityId;
            FullName = fullname;
            BaseFullName = baseFullName;

            // derived properties
            string[] parts = fullname.Split('.');
            IntfName = parts.Last();
            NameSpace = string.Join(".", parts.Take(parts.Length - 1));
        }
    }
}
