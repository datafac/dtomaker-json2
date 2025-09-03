using DTOMaker.SrcGen.Core;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace DTOMaker.SrcGen.JsonSystemText
{
    internal class JsonSTSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes) { }
        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes) { }
        public JsonSTSyntaxReceiver() : base(new JsonSTFactory()) { }
    }
}
