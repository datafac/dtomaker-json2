using System.Collections.Generic;

namespace DTOMaker.SrcGen.Core
{
    public interface IModelScope
    {
        IReadOnlyDictionary<string, object?> Tokens { get; }
    }
}
