using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace DTOMaker.SrcGen.Core
{
    public abstract class SourceGeneratorBase : IIncrementalGenerator
    {
        protected abstract void OnBeginInitialize(IncrementalGeneratorInitializationContext context);
        protected abstract void OnEndInitialize(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<MarkedInterface> markedInterfaces);

        // determine the namespace the syntax node is declared in, if any
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

        //private const string DomainAttribute = nameof(DomainAttribute);
        private const string EntityAttribute = nameof(EntityAttribute);
        private const string MemberAttribute = nameof(MemberAttribute);
        private const string IdAttribute = nameof(IdAttribute);

        private static bool FilterEntity(SyntaxNode syntaxNode, CancellationToken _)
        {
            return syntaxNode is InterfaceDeclarationSyntax;
        }

        protected static SyntaxDiagnostic? TryGetAttributeArgumentValue<T>(AttributeData attrData, Location location, int index, Action<T> action)
        {
            object? input = attrData.ConstructorArguments[index].Value;
            if (input is T value)
            {
                action(value);
                return null;
            }

            string inputAsStr = input is null ? "(null)" : $"'{input}' <{input.GetType().Name}>";

            return
                new SyntaxDiagnostic(
                    DiagnosticId.DTOM0005, "Invalid argument value", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    $"Could not read arg[{index}] {inputAsStr} as <{typeof(T).Name}>");
        }

        private static SyntaxDiagnostic? CheckAttributeArguments(AttributeData attrData, Location location, int expectedCount)
        {
            var attrArgs = attrData.ConstructorArguments;
            if (attrArgs.Length == expectedCount)
                return null;

            return new SyntaxDiagnostic(
                    DiagnosticId.DTOM0002, "Invalid argument count", DiagnosticCategory.Syntax, location, DiagnosticSeverity.Error,
                    $"Expected {attrData.AttributeClass?.Name} attribute to have {expectedCount} arguments, but it has {attrArgs.Length}.");
        }

        private static MarkedInterface GetMarkedInterface(GeneratorAttributeSyntaxContext ctx)
        {
            List<SyntaxDiagnostic> syntaxErrors = new();
            SemanticModel semanticModel = ctx.SemanticModel;
            SyntaxNode syntaxNode = ctx.TargetNode;
            Location location = syntaxNode.GetLocation();

            if (syntaxNode is not InterfaceDeclarationSyntax intfDeclarationSyntax)
            {
                // something went wrong
                return default;
            }

            // Get the semantic representation of the enum syntax
            if (semanticModel.GetDeclaredSymbol(intfDeclarationSyntax) is not INamedTypeSymbol intfSymbol)
            {
                // something went wrong
                return default;
            }

            // Get the namespace the enum is declared in, if any
            string generatedNamespace = GetNamespace(intfDeclarationSyntax);
            int entityId = 0;

            // Loop through all of the attributes on the interface
            foreach (AttributeData attributeData in intfSymbol.GetAttributes())
            {
                string? attrName = attributeData.AttributeClass?.Name;
                SyntaxDiagnostic? diagnostic = null;
                switch (attrName)
                {
                    case null:
                        break;
                    //DomainAttribute => null,
                    case EntityAttribute:
                        break;
                    case MemberAttribute:
                        break;
                    case IdAttribute:
                        // get entity id
                        diagnostic =
                            CheckAttributeArguments(attributeData, location, 1)
                            ?? TryGetAttributeArgumentValue<int>(attributeData, location, 0, (value) => { entityId = value; });
                        break;
                    default:
                        // todo pass to derived
                        diagnostic = new SyntaxDiagnostic(
                            "WRN001", "Ignored unknown attribute", DiagnosticCategory.Other, location, DiagnosticSeverity.Warning,
                            $"The attribute '{attrName}' is not recognized.");
                        break;
                }

                if (diagnostic is not null)
                {
                    syntaxErrors.Add(diagnostic);
                }

                // This is the attribute, check all of the named arguments
                //foreach (KeyValuePair<string, TypedConstant> namedArgument in attributeData.NamedArguments)
                //{
                //    // Is this the ExtensionClassName argument?
                //    if (namedArgument.Key == "ExtensionClassName"
                //        && namedArgument.Value.Value is not null)
                //    {
                //        //generatedClassName = namedArgument.Value.Value.ToString();
                //        break;
                //    }
                //}
            }

            if (entityId <= 0)
            {
                syntaxErrors.Add(new SyntaxDiagnostic(
                    "ERR001", "Missing or invalid Id", DiagnosticCategory.Syntax, syntaxNode.GetLocation(), DiagnosticSeverity.Error,
                    $"The interface '{intfSymbol.Name}' must have a valid Id attribute with a positive integer value."));
            }

            // Get the full type name of the enum e.g. Colour, 
            // or OuterClass<T>.Colour if it was nested in a generic type (for example)
            string fullname = intfSymbol.ToString();

            // Get all the members in the enum
            ImmutableArray<ISymbol> intfMembers = intfSymbol.GetMembers();
            var members = new List<string>(intfMembers.Length);

            // Get all the fields from the enum, and add their name to the list
            foreach (ISymbol member in intfMembers)
            {
                if (member is IFieldSymbol field && field.ConstantValue is not null)
                {
                    members.Add(member.Name);
                }
            }

            return new MarkedInterface(intfDeclarationSyntax, fullname, entityId, members, generatedNamespace, syntaxErrors.ToImmutableArray());
        }

        static void EmitEntityDiagnostics(SourceProductionContext context, MarkedInterface markedInterface)
        {
            foreach (SyntaxDiagnostic err in markedInterface.SyntaxErrors)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(err.Id, err.Title, err.Message,
                            err.Category, err.Severity, true), err.Location));
            }
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // do derived stuff
            OnBeginInitialize(context);

            // filter for entities
            IncrementalValuesProvider<MarkedInterface> markedInterfaces = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "DTOMaker.Models.EntityAttribute",
                    predicate: FilterEntity,
                    transform: static (ctx, _) => GetMarkedInterface(ctx))
                .Where(static m => m.IsValid);

            // validate entities
            context.RegisterSourceOutput(
                markedInterfaces,
                static (spc, markedInterface) => EmitEntityDiagnostics(spc, markedInterface));

            // generate summary
            context.RegisterSourceOutput(markedInterfaces.Collect(), (spc, interfaces) =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("// <auto-generated/>");
                sb.AppendLine("// Marked types summary.");
                sb.AppendLine("// Interfaces:");
                foreach (var intf in interfaces)
                {
                    sb.AppendLine($"// - {intf.NameSpace}.{intf.IntfName}");
                }
                sb.AppendLine("// End of summary.");
                spc.AddSource("Metadata.Summary.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            });

            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                // This is a way to check that the source generator is running
                // You can remove this diagnostic if you don't need it
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticsEN.OK01, Location.None));
            });

            // do derived stuff
            OnEndInitialize(context, markedInterfaces);
        }
    }

}