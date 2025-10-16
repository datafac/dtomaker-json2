using System.Collections.Generic;

namespace DTOMaker.SrcGen.Core
{
    public readonly record struct OutputEntity
    {
        public readonly string NameSpace;
        public readonly string FullName;
        public readonly string IntfName;
        public readonly int EntityId;
        public readonly EquatableArray<OutputMember> Members;

        public readonly int ClassHeight;

        public OutputEntity(string nameSpace, string fullname, string intfName, int entityId, IEnumerable<OutputMember> members, int classHeight)
        {
            NameSpace = nameSpace;
            FullName = fullname;
            IntfName = intfName;
            EntityId = entityId;
            Members = new(members);
            ClassHeight = classHeight;
        }
    }
}
