using DTOMaker.SrcGen.Core;
using Shouldly;
using System;
using System.Collections.Immutable;
using Xunit;

namespace DTOMaker.SrcGen.JsonSystemText.Tests
{
    public class InternalPipelineTests
    {
        private static readonly string modelSource =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Runtime;
            namespace MyOrg.Models
            {
                [Entity][Id(1)]
                public interface IDTO1 : IEntityBase
                {
                    [Member(1)] int  Field11 { get; set; }
                    [Member(2)] int? Field12 { get; set; }
                }
                [Entity][Id(2)]
                public interface IDTO2 : IDTO1
                {
                    [Member(1)] String  Field21 { get; set; }
                    [Member(2)] String? Field22 { get; set; }
                }
            }
            """;

        private static readonly TypeFullName EntityBase = 
            new TypeFullName(
                new ParsedName("DTOMaker.Runtime.IEntityBase"),
                new ParsedName("DTOMaker.Runtime.JsonSystemText.EntityBase"),
                MemberKind.Entity);

        private static TypeFullName CreateTFN(string name) =>
            new TypeFullName(
                new ParsedName($"MyOrg.Models.I{name}"),
                new ParsedName($"MyOrg.Models.{name}"),
                MemberKind.Entity);

        private static ParsedEntity CreateEntity(string name, int id, string? baseName) =>
            new ParsedEntity(CreateTFN(name), id, baseName is null ? EntityBase : CreateTFN(baseName));

        private static ParsedMember CreateMember(string entName, string fieldName, int sequence, Type type, bool isNullable = false)
        {
            TypeFullName memberType;
            if (type == typeof(int))
            {
                memberType = new TypeFullName(
                    new ParsedName("System.Int32"),
                    new ParsedName("System.Int32"),
                    MemberKind.Native);
            }
            else if (type == typeof(string))
            {
                memberType = new TypeFullName(
                    new ParsedName("System.String"),
                    new ParsedName("System.String"),
                    MemberKind.Native);
            }
            else
            {
                throw new NotSupportedException($"Type {type.FullName} not supported in test");
            }
            return new ParsedMember(
                    $"MyOrg.Models.I{entName}.{fieldName}", 
                    sequence,
                    memberType,
                    memberType.MemberKind, 
                    isNullable,
                    false, "", false);
        }

        private static readonly ImmutableArray<ParsedEntity> input = ImmutableArray<ParsedEntity>.Empty
                .Add(CreateEntity("DTO1", 1, null))
                .Add(CreateEntity("DTO2", 2, "DTO1"));

        private static readonly ImmutableArray<ParsedMember> members = ImmutableArray<ParsedMember>.Empty
                .Add(CreateMember("DTO1", "Field11", 1, typeof(int), false))
                .Add(CreateMember("DTO1", "Field12", 2, typeof(int), true))
                .Add(CreateMember("DTO2", "Field21", 1, typeof(string), false))
                .Add(CreateMember("DTO2", "Field22", 2, typeof(string), true));

        [Fact]
        public void Pipeline00_VerifyInput()
        {
            // arrange

            // act

            // assert
            input.Length.ShouldBe(2);
            input[0].TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO1");
            input[0].TFN.Impl.FullName.ShouldBe("MyOrg.Models.DTO1");
            input[0].BaseTFN.ShouldNotBeNull();
            input[0].BaseTFN.ToString().ShouldBe("DTOMaker.Runtime.JsonSystemText.EntityBase : DTOMaker.Runtime.IEntityBase");
            input[0].EntityId.ShouldBe(1);
            input[1].TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO2");
            input[1].TFN.Impl.FullName.ShouldBe("MyOrg.Models.DTO2");
            input[1].BaseTFN.ShouldNotBeNull();
            input[1].BaseTFN.ToString().ShouldBe("MyOrg.Models.DTO1 : IDTO1");
            input[1].EntityId.ShouldBe(2);
        }

        [Fact]
        public void Pipeline01_AddEntityBase()
        {
            // arrange
            input.Length.ShouldBe(2);

            // act
            var parsedEntities = SourceGeneratorBase.AddEntityBase(input);

            // assert
            parsedEntities.Length.ShouldBe(3);
            parsedEntities[0].TFN.Intf.FullName.ShouldBe("DTOMaker.Runtime.IEntityBase");
            parsedEntities[0].TFN.Impl.FullName.ShouldBe("DTOMaker.Runtime.JsonSystemText.EntityBase");
            parsedEntities[0].BaseTFN.ShouldBeNull();
            parsedEntities[0].EntityId.ShouldBe(0);

            parsedEntities[1].TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO1");
            parsedEntities[1].TFN.Impl.FullName.ShouldBe("MyOrg.Models.DTO1");
            parsedEntities[1].BaseTFN.ShouldNotBeNull();
            parsedEntities[1].BaseTFN.ToString().ShouldBe("DTOMaker.Runtime.JsonSystemText.EntityBase : DTOMaker.Runtime.IEntityBase");
            parsedEntities[1].EntityId.ShouldBe(1);

            parsedEntities[2].TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO2");
            parsedEntities[2].TFN.Impl.FullName.ShouldBe("MyOrg.Models.DTO2");
            parsedEntities[2].BaseTFN.ShouldNotBeNull();
            parsedEntities[2].BaseTFN.ToString().ShouldBe("MyOrg.Models.DTO1 : IDTO1");
            parsedEntities[2].EntityId.ShouldBe(2);
        }

        [Fact]
        public void Pipeline02_ResolveMembers()
        {
            // arrange
            input.Length.ShouldBe(2);

            // act
            var parsedEntities = SourceGeneratorBase.AddEntityBase(input);
            parsedEntities.Length.ShouldBe(3);

            var result0 = SourceGeneratorBase.ResolveMembers(parsedEntities[0], members, parsedEntities);
            var result1 = SourceGeneratorBase.ResolveMembers(parsedEntities[1], members, parsedEntities);
            var result2 = SourceGeneratorBase.ResolveMembers(parsedEntities[2], members, parsedEntities);

            // assert
            result0.TFN.Intf.FullName.ShouldBe("DTOMaker.Runtime.IEntityBase");
            result0.Members.Count.ShouldBe(0);

            result1.TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO1");
            result1.Members.Count.ShouldBe(2);
            result1.Members.Array[0].Name.ShouldBe("Field11");
            result1.Members.Array[0].Sequence.ShouldBe(1);
            result1.Members.Array[0].MemberType.Impl.Name.ShouldBe("Int32");
            result1.Members.Array[0].IsNullable.ShouldBe(false);
            result1.Members.Array[1].Name.ShouldBe("Field12");
            result1.Members.Array[1].Sequence.ShouldBe(2);
            result1.Members.Array[1].MemberType.Impl.Name.ShouldBe("Int32");
            result1.Members.Array[1].IsNullable.ShouldBe(true);

            result2.TFN.Intf.FullName.ShouldBe("MyOrg.Models.IDTO2");
            result2.Members.Count.ShouldBe(2);
            result2.Members.Array[0].Name.ShouldBe("Field21");
            result2.Members.Array[0].Sequence.ShouldBe(1);
            result2.Members.Array[0].MemberType.Impl.Name.ShouldBe("String");
            result2.Members.Array[0].IsNullable.ShouldBe(false);
            result2.Members.Array[1].Name.ShouldBe("Field22");
            result2.Members.Array[1].Sequence.ShouldBe(2);
            result2.Members.Array[1].MemberType.Impl.Name.ShouldBe("String");
            result2.Members.Array[1].IsNullable.ShouldBe(true);
        }

        [Fact]
        public void Pipeline03_ResolveEntities()
        {
            // arrange
            input.Length.ShouldBe(2);

            // act
            var parsedEntities = SourceGeneratorBase.AddEntityBase(input);
            parsedEntities.Length.ShouldBe(3);

            var entity0 = SourceGeneratorBase.ResolveMembers(parsedEntities[0], members, parsedEntities);
            var entity1 = SourceGeneratorBase.ResolveMembers(parsedEntities[1], members, parsedEntities);
            var entity2 = SourceGeneratorBase.ResolveMembers(parsedEntities[2], members, parsedEntities);

            var entities = ImmutableArray.Create<Phase1Entity>(entity0, entity1, entity2);

            var result0 = SourceGeneratorBase.ResolveEntities(entity0, entities);
            var result1 = SourceGeneratorBase.ResolveEntities(entity1, entities);
            var result2 = SourceGeneratorBase.ResolveEntities(entity2, entities);

            // assert
            result0.TFN.FullName.ShouldBe("DTOMaker.Runtime.JsonSystemText.EntityBase");
            result0.BaseEntity.ShouldBeNull();
            result0.ClassHeight.ShouldBe(0);
            result0.Members.Count.ShouldBe(0);
            result0.DerivedEntities.Count.ShouldBe(2);

            result1.TFN.FullName.ShouldBe("MyOrg.Models.DTO1");
            result1.BaseEntity.ShouldNotBeNull();
            result1.BaseEntity.TFN.FullName.ShouldBe("DTOMaker.Runtime.JsonSystemText.EntityBase");
            result1.ClassHeight.ShouldBe(1);
            result1.Members.Count.ShouldBe(2);
            result1.DerivedEntities.Count.ShouldBe(1);

            result2.TFN.FullName.ShouldBe("MyOrg.Models.DTO2");
            result2.BaseEntity.ShouldNotBeNull();
            result2.BaseEntity.TFN.FullName.ShouldBe("MyOrg.Models.DTO1");
            result2.ClassHeight.ShouldBe(2);
            result2.Members.Count.ShouldBe(2);
            result2.DerivedEntities.Count.ShouldBe(0);
        }
    }
}