namespace DTOMaker.SrcGen.Core
{
    public record class OutputEntity
    {
        public ParsedName Intf { get; init; } = new();
        public string ImplName => Intf.Name.StartsWith("I") ? Intf.Name.Substring(1) : Intf.Name;
        public int EntityId { get; init; }
        public int ClassHeight { get; init; }
        public EquatableArray<OutputMember> Members { get; init; } = EquatableArray<OutputMember>.Empty;
        public Phase1Entity? BaseEntity { get; init; }
        public EquatableArray<Phase1Entity> DerivedEntities { get; init; } = EquatableArray<Phase1Entity>.Empty;
    }
}
