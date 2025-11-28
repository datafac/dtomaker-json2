using System;

namespace DTOMaker.SrcGen.Core
{
    public record class ParsedEntity
    {
        public ParsedName Intf { get; init; } = new();
        public ParsedName Impl { get; init; } = new();
        public int EntityId { get; init; } = 0;
        public ParsedName? Base { get; init; } = null;

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
