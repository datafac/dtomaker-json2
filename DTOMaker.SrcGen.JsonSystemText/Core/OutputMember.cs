namespace DTOMaker.SrcGen.Core
{
    public readonly record struct OutputMember
    {
        public readonly string PropName;
        public readonly int Sequence;

        public OutputMember(string propName, int sequence)
        {
            PropName = propName;
            Sequence = sequence;
        }
    }
}
