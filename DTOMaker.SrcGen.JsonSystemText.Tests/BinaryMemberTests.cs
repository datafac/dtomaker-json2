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
    public class BinaryMemberTests
    {
        private readonly string inputSource1 =
            """
            using System;
            using DataFac.Memory;
            using DTOMaker.Models;
            using DTOMaker.Runtime;
            namespace MyOrg.Models
            {
                [Entity][Id(1)] public interface IMyDTO : IEntityBase
                {
                    [Member(1)] Octets  Field1 { get; set; }
                    [Member(2)] Octets? Field2 { get; set; }
                }
            }
            """;

        [Fact] public void BinaryMember_GeneratedSourcesLength() => inputSource1.GenerateAndCheckLength(2);
        [Fact] public async Task BinaryMember_VerifyGeneratedSource0() => await Verifier.Verify(inputSource1.GenerateAndGetOutput(0, "MyOrg.Models.JsonSystemText.EntityBase.g.cs"));
        [Fact] public async Task BinaryMember_VerifyGeneratedSource1() => await Verifier.Verify(inputSource1.GenerateAndGetOutput(1, "MyOrg.Models.JsonSystemText.MyDTO.g.cs"));

    }
}