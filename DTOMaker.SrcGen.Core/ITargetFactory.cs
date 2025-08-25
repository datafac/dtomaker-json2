using Microsoft.CodeAnalysis;

namespace DTOMaker.SrcGen.Core
{
    public interface ITargetFactory
    {
        TargetDomain CreateDomain(string name, Location location);
        TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location);
        TargetMember CreateMember(TargetEntity entity, string name, Location location);
        TargetMember CloneMember(TargetEntity entity, TargetMember source);
    }
}
