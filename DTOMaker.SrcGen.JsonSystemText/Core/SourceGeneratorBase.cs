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
        protected abstract void OnEndInitialize(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<OutputEntity> model);

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
        private const string ObsoleteAttribute = nameof(ObsoleteAttribute);

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

        private static ParsedMember GetParsedMember(GeneratorAttributeSyntaxContext ctx)
        {
            //List<SyntaxDiagnostic> syntaxErrors = new();
            SemanticModel semanticModel = ctx.SemanticModel;
            SyntaxNode syntaxNode = ctx.TargetNode;
            Location location = syntaxNode.GetLocation();

            if (syntaxNode is not PropertyDeclarationSyntax propDeclarationSyntax)
            {
                // something went wrong
                return default;
            }

            // Get the semantic representation of the enum syntax
            ISymbol? declSynbol = semanticModel.GetDeclaredSymbol(propDeclarationSyntax);
            if (declSynbol is not IPropertySymbol propSymbol)
            {
                // something went wrong
                return default;
            }

            // Get the namespace the enum is declared in, if any
            int sequence = 0;
            bool isObsolete = false;
            string obsoleteMessage = string.Empty;
            bool obsoleteIsError = false;

            // Loop through all of the attributes on the interface
            foreach (AttributeData attributeData in propSymbol.GetAttributes())
            {
                string? attrName = attributeData.AttributeClass?.Name;
                SyntaxDiagnostic? diagnostic = null;
                switch (attrName)
                {
                    case null:
                        break;
                    case MemberAttribute:
                        // get sequence
                        diagnostic =
                            CheckAttributeArguments(attributeData, location, 1)
                            ?? TryGetAttributeArgumentValue<int>(attributeData, location, 0, (value) => { sequence = value; });
                        break;
                    case ObsoleteAttribute:
                        isObsolete = true;
                        var attributeArguments = attributeData.ConstructorArguments;
                        if (attributeArguments.Length == 1)
                        {
                            TryGetAttributeArgumentValue<string>(attributeData, location, 0, (value) => { obsoleteMessage = value; });
                        }
                        if (attributeArguments.Length == 2)
                        {
                            TryGetAttributeArgumentValue<string>(attributeData, location, 0, (value) => { obsoleteMessage = value; });
                            TryGetAttributeArgumentValue<bool>(attributeData, location, 1, (value) => { obsoleteIsError = value; });
                        }
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
                    //syntaxErrors.Add(diagnostic);
                }
            }

            if (sequence <= 0)
            {
                //syntaxErrors.Add(new SyntaxDiagnostic(
                //    "ERR001", "Missing or invalid member sequence", DiagnosticCategory.Syntax, syntaxNode.GetLocation(), DiagnosticSeverity.Error,
                //    $"The interface '{propSymbol.Name}' must have a valid Id attribute with a positive integer value."));
            }

            // Get the full type name of the enum e.g. Colour, 
            // or OuterClass<T>.Colour if it was nested in a generic type (for example)
            string fullname = propSymbol.ToString();

            (TypeFullName tfn, MemberKind kind, bool isNullable) = GetTypeInfo(propSymbol.Type);

            return new ParsedMember(fullname, sequence, tfn, kind, isNullable, isObsolete, obsoleteMessage, obsoleteIsError);
        }

        private static (TypeFullName tfn, MemberKind kind, bool isNullable) GetTypeInfo(ITypeSymbol typeSymbol)
        {
            TypeFullName tfn = TypeFullName.Create(typeSymbol);
            MemberKind kind = tfn.MemberKind;
            bool isNullable = false;
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                if (namedTypeSymbol.IsGenericType && namedTypeSymbol.Name == "Nullable" && namedTypeSymbol.TypeArguments.Length == 1)
                {
                    // nullable value type
                    isNullable = true;
                    ITypeSymbol typeArg0 = namedTypeSymbol.TypeArguments[0];
                    tfn = TypeFullName.Create(typeArg0);
                    kind = tfn.MemberKind;
                }
            }
            if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                // nullable ref type
                isNullable = true;
            }
            return (tfn, kind, isNullable);
        }

        private static MemberKind GetMemberKind(ITypeSymbol typeSymbol)
        {
            string fullname = typeSymbol.ToString();
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

        private static ParsedEntity GetParsedEntity(GeneratorAttributeSyntaxContext ctx)
        {
            //List<SyntaxDiagnostic> syntaxErrors = new();
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
            //string generatedNamespace = GetNamespace(intfDeclarationSyntax);
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
                    //syntaxErrors.Add(diagnostic);
                }
            }

            if (entityId <= 0)
            {
                //syntaxErrors.Add(new SyntaxDiagnostic(
                //    "ERR001", "Missing or invalid Id", DiagnosticCategory.Syntax, syntaxNode.GetLocation(), DiagnosticSeverity.Error,
                //    $"The interface '{intfSymbol.Name}' must have a valid Id attribute with a positive integer value."));
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

            string? baseFullName = intfSymbol.Interfaces.FirstOrDefault()?.ToString();

            return new ParsedEntity(fullname, entityId, baseFullName);
        }

        private static int GetClassHeight(string? baseFullName, ImmutableArray<ParsedEntity> entities)
        {
            if (baseFullName is null) return 0;
            var parentEntity = entities.FirstOrDefault(e => e.FullName == baseFullName);
            if (!parentEntity.IsValid) return 0;
            return 1 + GetClassHeight(parentEntity.BaseFullName, entities);
        }

        private static List<Phase1Entity> GetDerivedEntities(string parentEntity, ImmutableArray<Phase1Entity> allEntities)
        {
            var derivedEntities = new List<Phase1Entity>();
            foreach (var entity in allEntities)
            {
                if (entity.BaseFullName == parentEntity)
                {
                    // found derived
                    derivedEntities.Add(entity);
                    // now recurse
                    derivedEntities.AddRange(GetDerivedEntities(entity.FullName, allEntities));
                }
            }
            return derivedEntities;
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // do derived stuff
            OnBeginInitialize(context);

            // filter for entities
            IncrementalValuesProvider<ParsedEntity> parsedEntities = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "DTOMaker.Models.EntityAttribute",
                    predicate: static (syntaxNode, _) => syntaxNode is InterfaceDeclarationSyntax,
                    transform: static (ctx, _) => GetParsedEntity(ctx))
                .Where(static m => m.IsValid);

            // add base entity
            parsedEntities = parsedEntities.Collect().Select((list1, _) =>
            {
                var baseEntity = new ParsedEntity("DTOMaker.Runtime.IEntityBase", 0, null);
                List<ParsedEntity> newList = [baseEntity];
                return newList.Concat(list1).ToImmutableArray();
            }).SelectMany((list2, _) => list2.ToImmutableArray());

            // filter for Members
            IncrementalValuesProvider<ParsedMember> parsedMembers = context.SyntaxProvider
                .ForAttributeWithMetadataName(
                    "DTOMaker.Models.MemberAttribute",
                    predicate: static (syntaxNode, _) => syntaxNode is PropertyDeclarationSyntax,
                    transform: static (ctx, _) => GetParsedMember(ctx))
                .Where(static m => m.IsValid);

            var parsedMatrix = parsedEntities.Collect().Combine(parsedMembers.Collect());

            // resolve members
            IncrementalValuesProvider<Phase1Entity> outputEntities1 = parsedEntities.Combine(parsedMatrix)
                .Select((pair, _) =>
                {
                    var parsed = pair.Left;
                    string prefix = parsed.FullName + ".";
                    var members = new List<OutputMember>();
                    foreach (ParsedMember member in pair.Right.Right)
                    {
                        if (member.FullName.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            members.Add(new OutputMember()
                            {
                                Name = member.PropName,
                                Sequence = member.Sequence,
                                MemberType = member.MemberType,
                                Kind = member.Kind,
                                IsNullable = member.IsNullable,
                                IsObsolete = member.IsObsolete,
                                ObsoleteMessage = member.ObsoleteMessage,
                                ObsoleteIsError = member.ObsoleteIsError,
                            });
                        }
                    }
                    int classHeight = GetClassHeight(parsed.BaseFullName, pair.Right.Left);
                    return new Phase1Entity()
                    {
                        FullName = parsed.FullName,
                        NameSpace = parsed.NameSpace,
                        IntfName = parsed.IntfName,
                        EntityId = parsed.EntityId,
                        ClassHeight = classHeight,
                        Members = new EquatableArray<OutputMember>(members.OrderBy(m => m.Sequence)),
                        BaseFullName = parsed.BaseFullName,
                    };
                });

            // resolve derived entities and height
            IncrementalValuesProvider<OutputEntity> outputEntities = outputEntities1.Combine(outputEntities1.Collect())
                .Select((pair, _) =>
                {
                    var entity = pair.Left;
                    var baseEntity = pair.Right.FirstOrDefault(e => e.FullName == entity.BaseFullName);
                    List<Phase1Entity> derivedEntities = GetDerivedEntities(entity.FullName, pair.Right);
                    return new OutputEntity()
                    {
                        FullName = entity.FullName,
                        NameSpace = entity.NameSpace,
                        IntfName = entity.IntfName,
                        EntityId = entity.EntityId,
                        ClassHeight = entity.ClassHeight,
                        Members = entity.Members,
                        BaseEntity = baseEntity,
                        DerivedEntities = new EquatableArray<Phase1Entity>(derivedEntities.OrderBy(e => e.FullName))
                    };
                });

            // generate summary
            //context.RegisterSourceOutput(outputEntities.Collect(), (spc, entities) =>
            //{
            //    var sb = new StringBuilder();
            //    sb.AppendLine("// <auto-generated/>");
            //    sb.AppendLine("// Entities:");
            //    foreach (var entity in entities)
            //    {
            //        sb.AppendLine($"// - {entity.NameSpace}.{entity.IntfName} ({entity.Members.Count} members)");
            //    }
            //    sb.AppendLine("// End.");
            //    spc.AddSource("Metadata.Summary.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            //});

            // todo emit metadata in json format
            //IncrementalValueProvider<string?> projectDirProvider = context.AnalyzerConfigOptionsProvider
            //    .Select(static (provider, _) =>
            //    {
            //        provider.GlobalOptions.TryGetValue("build_property.projectdir", out string? projectDirectory);
            //        return projectDirectory;
            //    });

            //string? projectDirectory = null;
            //context.RegisterSourceOutput(
            //    projectDirProvider,
            //    (context, source) =>
            //    {
            //        projectDirectory = source;
            //    });

            //IncrementalValuesProvider<ModelEntity> modelEntities = parsedEntities
            //    .Select((mi, _) => new ModelEntity
            //    {
            //        NameSpace = mi.NameSpace,
            //        IntfName = mi.IntfName,
            //        EntityId = mi.EntityId,
            //        Members = new EquatableArray<ModelMember>(mi.Values.Select(v => new ModelMember { PropName = v }).ToArray())
            //    });
            //context.RegisterSourceOutput(modelEntities.Collect(), (spc, entities) =>
            //{
            //    ModelMetadata metadata = new()
            //    {
            //        Entities = new EquatableArray<ModelEntity>(entities.ToArray())
            //    };
            //    string metadataText = metadata.ToString();
            //    spc.AddSource("Metadata.g.json", SourceText.From(metadataText, Encoding.UTF8));
            //});

            context.RegisterSourceOutput(context.CompilationProvider, (spc, compilation) =>
            {
                // This is a way to check that the source generator is running
                // You can remove this diagnostic if you don't need it
                spc.ReportDiagnostic(Diagnostic.Create(DiagnosticsEN.OK01, Location.None));
            });

            // do derived stuff
            OnEndInitialize(context, outputEntities);
        }
    }

}
