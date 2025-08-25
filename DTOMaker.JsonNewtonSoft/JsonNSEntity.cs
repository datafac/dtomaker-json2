using DTOMaker.Gentime;
using Microsoft.CodeAnalysis;

namespace DTOMaker.JsonSystemText
{
    internal sealed class JsonNSEntity : TargetEntity
    {
        public JsonNSEntity(TargetDomain domain, TypeFullName entityName, Location location)
            : base(domain, entityName, location) { }
    }
}
