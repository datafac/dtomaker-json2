using DTOMaker.SrcGen.Core;
using Microsoft.CodeAnalysis;

namespace DTOMaker.SrcGen.JsonSystemText
{
    internal sealed class JsonSTEntity : TargetEntity
    {
        public JsonSTEntity(TargetDomain domain, TypeFullName entityName, Location location)
            : base(domain, entityName, location) { }
    }
}
