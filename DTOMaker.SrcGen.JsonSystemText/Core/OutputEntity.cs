namespace DTOMaker.SrcGen.Core
{
    public record class OutputEntity
    {
        public string FullName { get; init; } = string.Empty;
        public string NameSpace { get; init; } = string.Empty;
        public string IntfName { get; init; } = string.Empty;
        public string ImplName => IntfName.StartsWith("I") ? IntfName.Substring(1) : IntfName;
        public int EntityId { get; init; }
        public int ClassHeight { get; init; }
        public EquatableArray<OutputMember> Members { get; init; } = EquatableArray<OutputMember>.Empty;
        public Phase1Entity? BaseEntity { get; init; }
        public EquatableArray<Phase1Entity> DerivedEntities { get; init; } = EquatableArray<Phase1Entity>.Empty;
    }
}
