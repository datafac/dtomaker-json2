namespace DTOMaker.SrcGen.Core
{
    public record class Phase1Entity
    {
        public ParsedName Intf { get; init; } = new();
        public ParsedName Impl { get; init; } = new();
        public int EntityId { get; init; }
        public int ClassHeight { get; init; }
        public EquatableArray<OutputMember> Members { get; init; } = EquatableArray<OutputMember>.Empty;
        public ParsedName? Base { get; init; }
    }
}
