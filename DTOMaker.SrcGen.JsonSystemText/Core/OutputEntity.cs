namespace DTOMaker.SrcGen.Core
{
    public record class OutputEntity
    {
        public ParsedName Intf { get; init; } = new();
        public ParsedName Impl { get; init; } = new();
        public int EntityId { get; init; }
        public int ClassHeight { get; init; }
        public EquatableArray<OutputMember> Members { get; init; } = EquatableArray<OutputMember>.Empty;
        public Phase1Entity? BaseEntity { get; init; }
        public EquatableArray<Phase1Entity> DerivedEntities { get; init; } = EquatableArray<Phase1Entity>.Empty;
    }
}
