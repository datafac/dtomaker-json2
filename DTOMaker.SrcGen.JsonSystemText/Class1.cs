using DTOMaker.SrcGen.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DTOMaker.SrcGen.JsonSystemText
{
    internal sealed class JsonNSDomain : TargetDomain
    {
        public JsonNSDomain(string name, Location location) : base(name, location) { }
    }
    internal sealed class JsonNSEntity : TargetEntity
    {
        public JsonNSEntity(TargetDomain domain, TypeFullName entityName, Location location)
            : base(domain, entityName, location) { }
    }
    internal sealed class JsonNSMember : TargetMember
    {
        public JsonNSMember(TargetEntity entity, string name, Location location) : base(entity, name, location) { }
        public JsonNSMember(TargetEntity entity, JsonNSMember source) : base(entity, source) { }
    }
    internal class JsonNSFactory : ITargetFactory
    {
        public TargetDomain CreateDomain(string name, Location location) => new JsonNSDomain(name, location);
        public TargetEntity CreateEntity(TargetDomain domain, TypeFullName tfn, Location location) => new JsonNSEntity(domain, tfn, location);
        public TargetMember CreateMember(TargetEntity entity, string name, Location location) => new JsonNSMember(entity, name, location);
        public TargetMember CloneMember(TargetEntity entity, TargetMember source) => new JsonNSMember(entity, (JsonNSMember)source);
    }
    internal class JsonNSSyntaxReceiver : SyntaxReceiverBase
    {
        protected override void OnProcessEntityAttributes(TargetEntity entity, Location location, ImmutableArray<AttributeData> entityAttributes) { }
        protected override void OnProcessMemberAttributes(TargetMember member, Location location, ImmutableArray<AttributeData> memberAttributes) { }
        public JsonNSSyntaxReceiver() : base(new JsonNSFactory()) { }
    }
    public sealed class JsonNSModelScopeDomain : ModelScopeDomain
    {
        public JsonNSModelScopeDomain(IModelScope parent, IScopeFactory factory, ILanguage language, TargetDomain domain)
            : base(parent, factory, language, domain)
        {
        }
    }
    public sealed class JsonNSModelScopeEntity : ModelScopeEntity
    {
        public JsonNSModelScopeEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
            : base(parent, factory, language, entity)
        {
        }
    }
    public sealed class JsonNSModelScopeMember : ModelScopeMember
    {
        public JsonNSModelScopeMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
            : base(parent, factory, language, member)
        {
        }
    }
    public sealed class JsonNSScopeFactory : IScopeFactory
    {
        public ModelScopeEntity CreateEntity(IModelScope parent, IScopeFactory factory, ILanguage language, TargetEntity entity)
        {
            return new JsonNSModelScopeEntity(parent, factory, language, entity);
        }

        public ModelScopeMember CreateMember(IModelScope parent, IScopeFactory factory, ILanguage language, TargetMember member)
        {
            return new JsonNSModelScopeMember(parent, factory, language, member);
        }
    }
    [Generator(LanguageNames.CSharp)]
    public class JsonNSSourceGenerator : SourceGeneratorBase
    {
        protected override void OnInitialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new JsonNSSyntaxReceiver());
        }

        private void EmitDiagnostics(GeneratorExecutionContext context, TargetBase target)
        {
            foreach (var diagnostic in target.SyntaxErrors)
            {
                // report diagnostic
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.Message,
                            diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
            foreach (var diagnostic in target.ValidationErrors())
            {
                // report diagnostic
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(diagnostic.Id, diagnostic.Title, diagnostic.Message,
                            diagnostic.Category, diagnostic.Severity, true), diagnostic.Location));
            }
        }

        protected override void OnExecute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not JsonNSSyntaxReceiver syntaxReceiver) return;

            //var assembly = Assembly.GetExecutingAssembly();
            var language = Language_CSharp.Instance;
            var factory = new JsonNSScopeFactory();

            var domain = syntaxReceiver.Domain;
            EmitDiagnostics(context, domain);

            var domainScope = new JsonNSModelScopeDomain(ModelScopeEmpty.Instance, factory, language, domain);

            // emit each entity
            foreach (var entity in domain.ClosedEntities.Values.OrderBy(e => e.TFN.FullName))
            {
                EmitDiagnostics(context, entity);
                foreach (var member in entity.Members.Values.OrderBy(m => m.Sequence))
                {
                    EmitDiagnostics(context, member);
                }

                var entityScope = factory.CreateEntity(domainScope, factory, language, entity);

                var generator = new EntityGenerator(language);
                string sourceText = generator.GenerateSourceText(entityScope);

                context.AddSource($"{entity.TFN.FullName}.JsonSystemText.g.cs", sourceText);
            }
        }
    }
}
