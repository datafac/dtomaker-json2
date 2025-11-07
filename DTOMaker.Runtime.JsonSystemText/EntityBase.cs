using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace DTOMaker.Runtime.JsonSystemText
{
    public abstract class EntityBaseOld : IEntityBase, IEquatable<EntityBaseOld>
    {
        protected abstract int OnGetEntityId();
        public int GetEntityId() => OnGetEntityId();

        public EntityBaseOld() { }
        public EntityBaseOld(IEntityBase notUsed) { }
        public EntityBaseOld(EntityBaseOld notUsed) { }
        private volatile bool _frozen;

        [JsonIgnore]
        public bool IsFrozen => _frozen;
        protected virtual void OnFreeze() { }
        public void Freeze()
        {
            if (_frozen) return;
            OnFreeze();
            _frozen = true;
        }
        protected abstract IEntityBase OnPartCopy();
        public IEntityBase PartCopy() => OnPartCopy();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowIsFrozenException(string? methodName) => throw new InvalidOperationException($"Cannot set {methodName} when frozen.");

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T IfNotFrozen<T>(T value, [CallerMemberName] string? methodName = null)
        {
            if (_frozen) ThrowIsFrozenException(methodName);
            return value;
        }

        public bool Equals(EntityBaseOld? other) => true;
        public override bool Equals(object? obj) => obj is EntityBaseOld;
        public override int GetHashCode() => HashCode.Combine<Type>(typeof(EntityBaseOld));

        protected static bool BinaryValuesAreEqual(byte[]? left, byte[]? right)
        {
            if (left is null) return (right is null);
            if (right is null) return false;
            return left.AsSpan().SequenceEqual(right.AsSpan());
        }

    }
}
