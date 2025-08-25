namespace DTOMaker.SrcGen.Core
{
    public interface IScopeFactory
    {
        ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity);
        ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member);
    }
}
