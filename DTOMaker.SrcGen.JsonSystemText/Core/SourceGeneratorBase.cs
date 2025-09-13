using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
    public readonly record struct EnumToGenerate
    {
        public readonly EnumDeclarationSyntax Syntax;
        public readonly string Name;
        public readonly EquatableArray<string> Values;

        public bool IsValid => !string.IsNullOrWhiteSpace(Name) && Values.Count > 0;

        public EnumToGenerate(EnumDeclarationSyntax syntax, string name, List<string> values)
        {
            Syntax = syntax;
            Name = name;
            Values = new(values.ToArray());
        }
    }
    public static class SourceGenerationHelper
    {
        public const string Attribute =
            """
            namespace NetEscapades.EnumGenerators
            {
                [System.AttributeUsage(System.AttributeTargets.Enum)]
                public class EnumExtensionsAttribute : System.Attribute
                {
                }
            }
            """;

        public static string GenerateExtensionClass(EnumToGenerate enumToGenerate)
        {
            string head =
                """
                namespace NetEscapades.EnumGenerators
                {
                    public static partial class EnumExtensions
                    {
                        public static string ToStringFast(this T_EnumName_ value)
                            => value switch
                            {
                """;
            string body =
                """
                                T_EnumName_.T_EnumMember_ => nameof(T_EnumName_.T_EnumMember_),
                """;
            string foot =
                """
                                _ => value.ToString()
                            }
                    }
                }
                """;
            var sb = new StringBuilder();
            sb.AppendLine(head.Replace("T_EnumName_", enumToGenerate.Name));
            foreach (string member in enumToGenerate.Values)
            {
                sb.AppendLine(body
                    .Replace("T_EnumName_", enumToGenerate.Name)
                    .Replace("T_EnumMember_", member));
            }
            sb.AppendLine(foot);
            return sb.ToString();
        }


    }

    public static class DiagnosticsEN
    {
        private static DiagnosticDescriptor CreateInfoDiagnostic(string cat, string id, string title, string desc)
        {
            return new DiagnosticDescriptor(
                id: id,
                title: title,
                messageFormat: desc,
                category: cat.ToString(),
                defaultSeverity: DiagnosticSeverity.Info,
                isEnabledByDefault: true);
        }

        private static readonly DiagnosticDescriptor _test01 = CreateInfoDiagnostic(DiagnosticCategory.Other, "TEST01", "A test diagnostic", "A description about the problem");
        public static DiagnosticDescriptor Test01 => _test01;
    }

    public abstract class SourceGeneratorBase : IIncrementalGenerator
    {
        protected abstract void OnInitialize(IncrementalGeneratorInitializationContext context);

        static EnumToGenerate GetEnumToGenerate(GeneratorAttributeSyntaxContext ctx)
        {
            SemanticModel semanticModel = ctx.SemanticModel;
            SyntaxNode syntaxNode = ctx.TargetNode;

            if (syntaxNode is not EnumDeclarationSyntax enumDeclarationSyntax)
            {
                // something went wrong
                return default;
            }

            // Get the semantic representation of the enum syntax
            if (semanticModel.GetDeclaredSymbol(enumDeclarationSyntax) is not INamedTypeSymbol enumSymbol)
            {
                // something went wrong
                return default;
            }

            // Get the full type name of the enum e.g. Colour, 
            // or OuterClass<T>.Colour if it was nested in a generic type (for example)
            string enumName = enumSymbol.ToString();

            // Get all the members in the enum
            ImmutableArray<ISymbol> enumMembers = enumSymbol.GetMembers();
            var members = new List<string>(enumMembers.Length);

            // Get all the fields from the enum, and add their name to the list
            foreach (ISymbol member in enumMembers)
            {
                if (member is IFieldSymbol field && field.ConstantValue is not null)
                {
                    members.Add(member.Name);
                }
            }

            // Create an EnumToGenerate for use in the generation phase
            //enumsToGenerate.Add(new EnumToGenerate(enumName, members));

            foreach (ISymbol member in enumMembers)
            {
                if (member is IFieldSymbol field && field.ConstantValue is not null)
                {
                    members.Add(member.Name);
                }
            }

            return new EnumToGenerate(enumDeclarationSyntax, enumName, members);
        }

        static void Execute(SourceProductionContext context, EnumToGenerate enumToGenerate)
        {
            // generate the source code and add it to the output
            string result = SourceGenerationHelper.GenerateExtensionClass(enumToGenerate);
            // Create a separate partial class file for each enum
            context.AddSource($"EnumExtensions.{enumToGenerate.Name}.g.cs", SourceText.From(result, Encoding.UTF8));

            // Add a dummy diagnostic
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticsEN.Test01, enumToGenerate.Syntax.GetLocation()));
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attributes to the compilation
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "EnumExtensionsAttribute.g.cs",
                SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

            // Do a simple filter for enums
            IncrementalValuesProvider<EnumToGenerate> enumsToGenerate = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "NetEscapades.EnumGenerators.EnumExtensionsAttribute",
                    predicate: static (s, _) => true,
                    transform: static (ctx, _) => GetEnumToGenerate(ctx))
                .Where(static m => m.IsValid);

            // Generate source code for each enum found
            context.RegisterSourceOutput(
                enumsToGenerate,
                static (spc, source) => Execute(spc, source));
            
            // now do derived stuff
            OnInitialize(context);
        }
    }

}