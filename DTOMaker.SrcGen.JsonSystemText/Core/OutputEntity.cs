namespace DTOMaker.SrcGen.Core
{
    public record class OutputEntity
    {
        public string FullName { get; init; } = string.Empty;
        public string NameSpace { get; init; } = string.Empty;
        public string IntfName { get; init; } = string.Empty;
        public int EntityId { get; init; }
        public int ClassHeight { get; init; }
        public EquatableArray<OutputMember> Members { get; init; } = EquatableArray<OutputMember>.Empty;
        public string? BaseFullName { get; init; }
        public EquatableArray<string> DerivedFullNames { get; init; } = EquatableArray<string>.Empty;
    }
}
