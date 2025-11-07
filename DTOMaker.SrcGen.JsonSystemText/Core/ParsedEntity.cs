using System;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public record class ParsedName
    {
        public string FullName { get; } = string.Empty;
        public string Space { get; } = string.Empty;
        public string Name { get; } = string.Empty;

        public ParsedName() { }
        public ParsedName(string fullname)
        {
            FullName = fullname;
            int lastDotPos = fullname.AsSpan().LastIndexOf('.');
            Space = lastDotPos < 0 ? string.Empty : fullname.AsSpan().Slice(0, lastDotPos).ToString();
            Name = lastDotPos < 0 ? fullname : fullname.AsSpan().Slice(lastDotPos + 1).ToString();
        }
    }

    public readonly record struct ParsedEntity
    {
        // todo convert to record class
        public readonly ParsedName Intf = new();
        public readonly int EntityId;
        public readonly string? BaseFullName;

        public bool IsValid => !string.IsNullOrWhiteSpace(Intf.Space) && !string.IsNullOrWhiteSpace(Intf.Name) && Intf.Name.StartsWith("I", StringComparison.Ordinal);

        public ParsedEntity(string fullname, int entityId, string? baseFullName)
        {
            Intf = new ParsedName(fullname);
            EntityId = entityId;
            BaseFullName = baseFullName;
        }
    }
}
