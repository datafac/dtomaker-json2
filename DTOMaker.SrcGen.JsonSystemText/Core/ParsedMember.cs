using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct ParsedMember
    {
        public readonly string FullName;
        public readonly string PropName;
        public readonly int Sequence;
        public readonly TypeFullName MemberType;
        public readonly MemberKind Kind;
        public readonly bool IsNullable;
        public readonly bool IsObsolete;
        public readonly string ObsoleteMessage;
        public readonly bool ObsoleteIsError;

        public bool IsValid => !string.IsNullOrWhiteSpace(PropName);

        public ParsedMember(string fullname, int sequence, TypeFullName memberType, MemberKind kind, bool isNullable, bool isObsolete, string obsoleteMessage, bool obsoleteIsError)
        {
            FullName = fullname;
            Sequence = sequence;
            MemberType = memberType;
            Kind = kind;
            IsNullable = isNullable;
            IsObsolete = isObsolete;
            ObsoleteMessage = obsoleteMessage;
            ObsoleteIsError = obsoleteIsError;

            // derived properties
            PropName = fullname.Split('.').Last();
        }
    }
}
