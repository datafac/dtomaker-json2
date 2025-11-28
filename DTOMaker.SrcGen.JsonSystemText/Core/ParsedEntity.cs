using Microsoft.CodeAnalysis;
using System;

namespace DTOMaker.SrcGen.Core
{
    public record class ParsedEntity
    {
        public TypeFullName TFN { get; init; } = new();
        public ParsedName Intf => TFN.Intf;
        public ParsedName Impl => TFN.Impl;
        public int EntityId { get; init; } = 0;
        public ParsedName? Base { get; init; } = null;

        public ParsedEntity(ITypeSymbol ids, int entityId, string? baseFullName)
        {
            TFN = new TypeFullName(ids);
            //Intf = new ParsedName(intfFullName);
            //string implName = Intf.Name.StartsWith("I", StringComparison.Ordinal) ? Intf.Name.Substring(1) : Intf.Name;
            //Impl = new ParsedName(Intf.Space + ".JsonSystemText", implName);
            EntityId = entityId;
            Base = baseFullName is null ? null : new ParsedName(baseFullName);
        }
        public ParsedEntity(ParsedName intf, ParsedName impl, MemberKind kind, int entityId, string? baseFullName)
        {
            TFN = new TypeFullName(intf, impl, kind);
            EntityId = entityId;
            Base = baseFullName is null ? null : new ParsedName(baseFullName);
        }
    }
}
