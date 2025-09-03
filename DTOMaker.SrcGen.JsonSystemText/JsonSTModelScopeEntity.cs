using DTOMaker.SrcGen.Core;

namespace DTOMaker.SrcGen.JsonSystemText
{
    public sealed class JsonSTModelScopeEntity : ModelScopeEntity
    {
        public JsonSTModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language, entity)
        {
        }
    }
}
