using DTOMaker.SrcGen.Core;

namespace DTOMaker.SrcGen.JsonSystemText
{
    public sealed class JsonSTModelScopeMember : ModelScopeMember
    {
        public JsonSTModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language, member)
        {
        }
    }
}
