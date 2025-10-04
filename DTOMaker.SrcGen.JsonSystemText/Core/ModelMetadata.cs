namespace DTOMaker.SrcGen.Core
{
    public sealed record ModelMetadata
    {
        public EquatableArray<ModelEntity> Entities { get; init; } = new();
    }
}
