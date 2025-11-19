using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.SrcGen.JsonSystemText.Tests
{
    public class EntityMemberTests
    {
        private readonly string modelSource =
            """
            using DTOMaker.Models;
            namespace MyOrg.DomainA
            {
                [Entity][Id(1)] public interface IMyDTO1
                {
                    [Member(1)] long Field1 { get; set; }
                }
            }
            namespace MyOrg.DomainB
            {
                [Entity][Id(2)] public interface IMyDTO1
                {
                    [Member(1)] double Field1 { get; set; }
                }
            }
            namespace MyOrg.DomainC
            {
                [Entity][Id(3)] public interface IMyDTO2
                {
                    [Member(1)] MyOrg.DomainA.IMyDTO1? Member1 { get; set; }
                    [Member(2)] MyOrg.DomainB.IMyDTO1  Member2 { get; set; }
                }
            }
            """;

        [Fact] public void EntityMember_GeneratedSourcesLength() => modelSource.GenerateAndCheckLength(4);
        //
        [Fact] public async Task EntityMember_VerifyGeneratedSource0() => await Verifier.Verify(modelSource.GenerateAndGetOutput(0, "MyOrg.DomainA.JsonSystemText.EntityBase.g.cs"));
        [Fact] public async Task EntityMember_VerifyGeneratedSource1() => await Verifier.Verify(modelSource.GenerateAndGetOutput(1, "MyOrg.DomainA.JsonSystemText.MyDTO1.g.cs"));
        [Fact] public async Task EntityMember_VerifyGeneratedSource2() => await Verifier.Verify(modelSource.GenerateAndGetOutput(2, "MyOrg.DomainB.JsonSystemText.MyDTO1.g.cs"));
        [Fact] public async Task EntityMember_VerifyGeneratedSource3() => await Verifier.Verify(modelSource.GenerateAndGetOutput(3, "MyOrg.DomainC.JsonSystemText.MyDTO2.g.cs"));

    }
}