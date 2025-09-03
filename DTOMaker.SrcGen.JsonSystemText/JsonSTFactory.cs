using DTOMaker.SrcGen.Core;
using Microsoft.CodeAnalysis;

namespace DTOMaker.SrcGen.JsonSystemText
{
    internal class JsonSTFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new JsonSTDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new JsonSTEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new JsonSTMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new JsonSTMember(entity, (JsonSTMember)source);
    }
}
