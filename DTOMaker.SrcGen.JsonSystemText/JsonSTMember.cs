using DTOMaker.SrcGen.Core;
using Microsoft.CodeAnalysis;

namespace DTOMaker.SrcGen.JsonSystemText
{
    internal sealed class JsonSTMember : TargetMember
    {
        public JsonSTMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public JsonSTMember(TargetEntity entity, JsonSTMember source) : base(entity, source) { }
    }
}
