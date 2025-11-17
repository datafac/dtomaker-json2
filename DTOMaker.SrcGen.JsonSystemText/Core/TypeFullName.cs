using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DTOMaker.SrcGen.Core
{
    public readonly struct TypeFullName : IEquatable<TypeFullName>
    {
        private readonly string _nameSpace;
        private readonly string _name;
        private readonly ImmutableArray<ITypeParameterSymbol> _typeParameters; // generics only
        private readonly ImmutableArray<ITypeSymbol> _typeArguments; // closed generics only
        private readonly string _fullName;
        private readonly int _syntheticId;
        private readonly MemberKind _memberKind;

        private static readonly TypeFullName _defaultBase = 
            new TypeFullName("DTOMaker.Runtime", "EntityBase", MemberKind.Entity,
            ImmutableArray<ITypeParameterSymbol>.Empty, ImmutableArray<ITypeSymbol>.Empty);
        public static TypeFullName DefaultBase => _defaultBase;

        private static int GetSyntheticId(string fullname)
        {
            return fullname switch
            {
                KnownType.SystemBoolean => 9001,
                KnownType.SystemSByte => 9002,
                KnownType.SystemByte => 9003,
                KnownType.SystemInt16 => 9004,
                KnownType.SystemUInt16 => 9005,
                KnownType.SystemChar => 9006,
                KnownType.SystemHalf => 9007,
                KnownType.SystemInt32 => 9008,
                KnownType.SystemUInt32 => 9009,
                KnownType.SystemSingle => 9010,
                KnownType.SystemInt64 => 9011,
                KnownType.SystemUInt64 => 9012,
                KnownType.SystemDouble => 9013,
                KnownType.SystemInt128 => 9014,
                KnownType.SystemUInt128 => 9015,
                KnownType.SystemGuid => 9016,
                KnownType.SystemDecimal => 9017,
                KnownType.SystemString => 9018,
                // custom types
                KnownType.PairOfInt16 => 9096,
                KnownType.PairOfInt32 => 9097,
                KnownType.PairOfInt64 => 9098,
                KnownType.MemoryOctets => 9099,
                _ => 0,
            };
        }

        private static MemberKind GetMemberKind(string fullname)
        {
            return fullname switch
            {
                KnownType.SystemBoolean => MemberKind.Native,
                KnownType.SystemSByte => MemberKind.Native,
                KnownType.SystemByte => MemberKind.Native,
                KnownType.SystemInt16 => MemberKind.Native,
                KnownType.SystemUInt16 => MemberKind.Native,
                KnownType.SystemChar => MemberKind.Native,
                KnownType.SystemHalf => MemberKind.Native,
                KnownType.SystemInt32 => MemberKind.Native,
                KnownType.SystemUInt32 => MemberKind.Native,
                KnownType.SystemSingle => MemberKind.Native,
                KnownType.SystemInt64 => MemberKind.Native,
                KnownType.SystemUInt64 => MemberKind.Native,
                KnownType.SystemDouble => MemberKind.Native,
                KnownType.SystemInt128 => MemberKind.Native,
                KnownType.SystemUInt128 => MemberKind.Native,
                KnownType.SystemGuid => MemberKind.Native,
                KnownType.SystemDecimal => MemberKind.Native,
                KnownType.SystemString => MemberKind.String,
                // custom types
                KnownType.PairOfInt16 => MemberKind.Native,
                KnownType.PairOfInt32 => MemberKind.Native,
                KnownType.PairOfInt64 => MemberKind.Native,
                KnownType.MemoryOctets => MemberKind.Binary,
                _ => MemberKind.Unknown,
            };
        }

        private TypeFullName(string nameSpace, string name, MemberKind kind, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            _nameSpace = nameSpace;
            _name = name;
            _typeParameters = typeParameters;
            _typeArguments = typeArguments;
            _fullName = nameSpace + "." + MakeCSImplName(name, typeParameters, typeArguments);
            _syntheticId = GetSyntheticId(_fullName);
            _memberKind = kind;
        }

        public TypeFullName(ITypeSymbol ids)
        {
            string nameSpace = ids.ContainingNamespace.ToDisplayString();
            string entityName = ids.Name;
            _typeParameters = ImmutableArray<ITypeParameterSymbol>.Empty;
            _typeArguments = ImmutableArray<ITypeSymbol>.Empty;
            if (ids.TypeKind == TypeKind.Interface)
            {
                entityName = ids.Name.Substring(1);
                if (ids is INamedTypeSymbol nts)
                {
                    _typeParameters = nts.TypeParameters;
                    _typeArguments = nts.TypeArguments;
                }
            }
            _nameSpace = nameSpace;
            _name = entityName;
            _fullName = _nameSpace + "." + MakeCSImplName(_name, _typeParameters, _typeArguments);
            _syntheticId = GetSyntheticId(_fullName);
            _memberKind = GetMemberKind(_fullName);
            if (_memberKind == MemberKind.Unknown && ids.TypeKind == TypeKind.Interface)
            {
                _memberKind = MemberKind.Entity;
            }
        }

        public string NameSpace => _nameSpace;
        public string ShortImplName => MakeCSImplName(_name, _typeParameters, _typeArguments);
        public string ShortIntfName => MakeCSIntfName(_name, _typeParameters, _typeArguments);
        public string FullName => _fullName;
        public int SyntheticId => _syntheticId;
        public MemberKind MemberKind => _memberKind;
        public bool IsGeneric => _typeParameters.Length > 0;
        public bool IsClosed => (_typeArguments.Length == _typeParameters.Length)
                                && _typeArguments.All(ta => ta.Kind != SymbolKind.TypeParameter);

        public ImmutableArray<ITypeParameterSymbol> TypeParameters => _typeParameters;
        public ImmutableArray<ITypeSymbol> TypeArguments => _typeArguments;
        public TypeFullName AsOpenGeneric()
        {
            return new TypeFullName(_nameSpace, _name, _memberKind, _typeParameters, ImmutableArray<ITypeSymbol>.Empty);
        }
        public TypeFullName AsClosedGeneric(ImmutableArray<ITypeSymbol> typeArguments)
        {
            return new TypeFullName(_nameSpace, _name, _memberKind, _typeParameters, typeArguments);
        }

        public bool Equals(TypeFullName other) => string.Equals(_fullName, other._fullName, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is TypeFullName other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(_fullName);
        public static bool operator ==(TypeFullName left, TypeFullName right) => left.Equals(right);
        public static bool operator !=(TypeFullName left, TypeFullName right) => !left.Equals(right);
        public override string ToString() => _fullName;

        /// <summary>
        /// Creates a unique entity name with closed generic arguments
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeParameters"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        private static string MakeCSImplName(string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            if (typeParameters.Length == 0) return name;

            StringBuilder result = new StringBuilder();
            result.Append(name);
            result.Append('_');
            result.Append(typeParameters.Length);
            for (int i = 0; i < typeParameters.Length; i++)
            {
                result.Append('_');
                if (i < typeArguments.Length && typeArguments[i].Kind == SymbolKind.NamedType)
                {
                    var aTFN = new TypeFullName(typeArguments[i]);
                    result.Append(aTFN.ShortImplName);
                }
                else
                {
                    //var tp = typeParameters[i];
                    result.Append($"T{i}");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Reconstructs the open or closed CSharp interface name. This should be the same as that given in the source model.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="typeParameters"></param>
        /// <param name="typeArguments"></param>
        /// <returns></returns>
        private static string MakeCSIntfName(string name, ImmutableArray<ITypeParameterSymbol> typeParameters, ImmutableArray<ITypeSymbol> typeArguments)
        {
            if (typeParameters.Length == 0) return name;

            StringBuilder result = new StringBuilder();
            result.Append(name);
            result.Append('<');
            for (int i = 0; i < typeParameters.Length; i++)
            {
                if (i > 0) result.Append(", ");
                if (i < typeArguments.Length)
                {
                    var ta = typeArguments[i];
                    result.Append(ta.Name);
                }
                else
                {
                    var tp = typeParameters[i];
                    result.Append(tp.Name);
                }
            }
            result.Append('>');
            return result.ToString();
        }
    }
}