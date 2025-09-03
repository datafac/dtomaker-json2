using DTOMaker.SrcGen.Core;

namespace DTOMaker.SrcGen.JsonSystemText
{
    public sealed class JsonSTModelScopeDomain : ModelScopeDomain
    {
        public JsonSTModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
}
