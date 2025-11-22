using DTOMaker.Models;
using DTOMaker.Runtime;
using DTOMaker.Runtime.JsonSystemText;
using DTOMaker.SrcGen.JsonSystemText.IntTests.JsonSystemText;
using Shouldly;
using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace DTOMaker.SrcGen.JsonSystemText.IntTests
{
    [Entity][Id(30)]
    public interface INode : IEntityBase
    {
        [Member(1)] String Key { get; set; }
    }

    [Entity][Id(31)]
    public interface IStringNode : INode
    {
        [Member(1)] String Value { get; set; }
    }

    [Entity][Id(32)]
    public interface INumberNode : INode
    {
        [Member(1)] Int64 Value { get; set; }
    }

    [Entity][Id(33)]
    public interface ITree : IEntityBase
    {
        [Member(1)] ITree? Left { get; set; }
        [Member(2)] ITree? Right { get; set; }
        [Member(3)] INode? Node { get; set; }
    }

    public class RecursiveGraphTests
    {
        public string Roundtrip_Graph(Tree orig)
        {
            orig.Freeze();
            var json = orig.SerializeToJson();
            var copy = json.DeserializeFromJson<Tree>();
            copy.ShouldNotBeNull();
            copy.ShouldBe(orig);
            return json;
        }

        [Fact]
        public async Task Roundtrip_Tree()
        {
            var tree = new Tree
            {
                Left = new Tree
                {
                    Node = new StringNode
                    {
                        Key = "left-key",
                        Value = "left-value"
                    }
                },
                Right = new Tree
                {
                    Node = new NumberNode
                    {
                        Key = "right-key",
                        Value = 314L
                    }
                },
                Node = new StringNode
                {
                    Key = "mid-key",
                    Value = "mid-value"
                }
            };
            string json = Roundtrip_Graph(tree);
            await Verifier.Verify(json);
        }
    }
}
