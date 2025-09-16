using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DTOMaker.SrcGen.Core
{
    public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>, IReadOnlyCollection<T>
        where T : IEquatable<T>
    {
        private readonly T[] _array;

        public EquatableArray() => _array = Array.Empty<T>();
        public EquatableArray(T[] array) => _array = array;

        public int Count => _array.Length;
        public ReadOnlySpan<T> AsSpan() => _array.AsSpan();
        public T[]? AsArray() => _array;


        public bool Equals(EquatableArray<T> array) => AsSpan().SequenceEqual(array.AsSpan());
        public override bool Equals(object? obj) => obj is EquatableArray<T> array && this.Equals(array);
        public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) => left.Equals(right);
        public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);

        public override int GetHashCode()
        {
            HashCode hashCode = default;
            hashCode.Add(_array.Length);
            for (int i = 0; i < _array.Length; i++)
            {
                hashCode.Add(_array[i]);
            }
            return hashCode.ToHashCode();
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)_array).GetEnumerator();
    }

}