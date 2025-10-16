using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedMember
    {
        public readonly string FullName;
        public readonly string PropName;
        public readonly int Sequence;

        public bool IsValid => !string.IsNullOrWhiteSpace(PropName);

        public ParsedMember(string fullname, int sequence)
        {
            FullName = fullname;
            Sequence = sequence;

            // derived properties
            PropName = fullname.Split('.').Last();
        }
    }
}
