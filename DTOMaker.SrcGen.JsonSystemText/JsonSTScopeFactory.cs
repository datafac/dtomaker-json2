using DTOMaker.SrcGen.Core;

namespace DTOMaker.SrcGen.JsonSystemText
{
    public sealed class JsonSTScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new JsonSTModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new JsonSTModelScopeMember(parent, factory, language, member);
        }
    }
}
