using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace DTOMaker.SrcGen.Core
{
    public abstract class SourceGeneratorBase : IIncrementalGenerator
    {
        protected abstract void OnInitialize(IncrementalGeneratorInitializationContext context);

        // todo remove this
        private const string AttributeSource =
            """
            namespace NetEscapades.EnumGenerators
            {
                [System.AttributeUsage(System.AttributeTargets.Enum)]
                public class EnumExtensionsAttribute : System.Attribute
                {
                    public string ExtensionClassName { get; set; }
                }
            }
            """;

        private static string GenerateExtensionClass(EnumToGenerate enumToGenerate)
        {
            string head =
                """
                namespace T_GeneratedNamespace_
                {
                    public static partial class T_GeneratedClassName_
                    {
                        public static string ToStringFast(this T_EnumName_ value)
                        {
                            return value switch
                            {
                """;
            string body =
                """
                                T_EnumName_.T_EnumMember_ => nameof(T_EnumName_.T_EnumMember_),
                """;
            string foot =
                """
                                _ => value.ToString()
                            };
                        }
                    }
                }
                """;
            var sb = new StringBuilder();
            sb.AppendLine(head
                .Replace("T_GeneratedNamespace_", enumToGenerate.GeneratedNamespace)
                .Replace("T_GeneratedClassName_", enumToGenerate.GeneratedClassName)
                .Replace("T_EnumName_", enumToGenerate.Name));
            foreach (string member in enumToGenerate.Values)
            {
                sb.AppendLine(body
                    .Replace("T_EnumName_", enumToGenerate.Name)
                    .Replace("T_EnumMember_", member));
            }
            sb.AppendLine(foot);
            return sb.ToString();
        }

        // determine the namespace the class/enum/struct is declared in, if any
        static string GetNamespace(BaseTypeDeclarationSyntax syntax)
        {
            // If we don't have a namespace at all we'll return an empty string
            // This accounts for the "default namespace" case
            string nameSpace = string.Empty;

            // Get the containing syntax node for the type declaration
            // (could be a nested type, for example)
            SyntaxNode? potentialNamespaceParent = syntax.Parent;

            // Keep moving "out" of nested classes etc until we get to a namespace
            // or until we run out of parents
            while (potentialNamespaceParent != null &&
                    potentialNamespaceParent is not NamespaceDeclarationSyntax
                    && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
            {
                potentialNamespaceParent = potentialNamespaceParent.Parent;
            }

            // Build up the final namespace by looping until we no longer have a namespace declaration
            if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
            {
                // We have a namespace. Use that as the type
                nameSpace = namespaceParent.Name.ToString();

                // Keep moving "out" of the namespace declarations until we 
                // run out of nested namespace declarations
                while (true)
                {
                    if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                    {
                        break;
                    }

                    // Add the outer namespace as a prefix to the final namespace
                    nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                    namespaceParent = parent;
                }
            }

            // return the final namespace
            return nameSpace;
        }

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

            // Get the namespace the enum is declared in, if any
            string generatedNamespace = GetNamespace(enumDeclarationSyntax);

            // Set the default extension name
            string generatedClassName = "EnumExtensions";

            // Loop through all of the attributes on the enum
            foreach (AttributeData attributeData in ctx.Attributes)
            {
                // This is the attribute, check all of the named arguments
                foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
                {
                    // Is this the ExtensionClassName argument?
                    if (namedArgument.Key == "ExtensionClassName"
                        && namedArgument.Value.Value is not null)
                    {
                        generatedClassName = namedArgument.Value.Value.ToString();
                        break;
                    }
                }
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

            return new EnumToGenerate(enumDeclarationSyntax, enumName, members, generatedClassName, generatedNamespace);
        }

        static void GenerateEnumExtensions(SourceProductionContext context, EnumToGenerate enumToGenerate)
        {
            // generate the source code and add it to the output
            string result = GenerateExtensionClass(enumToGenerate);
            // Create a separate partial class file for each enum
            context.AddSource($"EnumExtensions.{enumToGenerate.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Add the marker attributes to the compilation
            context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
                "EnumExtensionsAttribute.g.cs",
                SourceText.From(AttributeSource, Encoding.UTF8)));

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
                static (spc, source) => GenerateEnumExtensions(spc, source));

            var allEnums = enumsToGenerate.Collect();

            context.RegisterSourceOutput(allEnums, (spc, enums) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("// List of generated enums:");
                foreach (var enumToGenerate in enums)
                {
                    sb.AppendLine($"// - {enumToGenerate.Name} ({enumToGenerate.Values.Count} members) -> {enumToGenerate.GeneratedNamespace}.{enumToGenerate.GeneratedClassName}");
                }
                sb.AppendLine("// End of list.");
                spc.AddSource("EnumExtensions.Summary.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                // This is a way to check that the source generator is running
                // You can remove this diagnostic if you don't need it
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticsEN.OK01, Location.None));
            });

            // now do derived stuff
            OnInitialize(context);
        }
    }

}