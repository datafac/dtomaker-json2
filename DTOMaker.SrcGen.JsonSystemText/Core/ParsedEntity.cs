using System;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedEntity
    {
        // todo convert to record class
        public readonly ParsedName Intf = new();
        public readonly ParsedName Impl = new();
        public readonly int EntityId;
        public readonly ParsedName? Base;

        public bool IsValid => (Intf is not null) && !string.IsNullOrWhiteSpace(Intf.Space) && !string.IsNullOrWhiteSpace(Intf.Name) && Intf.Name.StartsWith("I", StringComparison.Ordinal);

        public ParsedEntity(string intfFullName, int entityId, string? baseFullName)
        {
            Intf = new ParsedName(intfFullName);
            string implName = Intf.Name.StartsWith("I", StringComparison.Ordinal) ? Intf.Name.Substring(1) : Intf.Name;
            Impl = new ParsedName(Intf.Space + ".JsonSystemText", implName);
            EntityId = entityId;
            Base = baseFullName is null ? null : new ParsedName(baseFullName);
        }
        public ParsedEntity(string intfFullName, string implSpace, int entityId, string? baseFullName)
        {
            Intf = new ParsedName(intfFullName);
            string implName = Intf.Name.StartsWith("I", StringComparison.Ordinal) ? Intf.Name.Substring(1) : Intf.Name;
            Impl = new ParsedName(implSpace, implName);
            EntityId = entityId;
            Base = baseFullName is null ? null : new ParsedName(baseFullName);
        }
    }
}
