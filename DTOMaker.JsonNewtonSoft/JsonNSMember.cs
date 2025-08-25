using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonSystemText
{
    internal sealed class JsonNSMember : TargetMember
    {
        public JsonNSMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public JsonNSMember(TargetEntity entity, JsonNSMember source) : base(entity, source) { }
    }
}
