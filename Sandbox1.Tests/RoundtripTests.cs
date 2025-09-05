using DTOMaker.Runtime.JsonSystemText;
using Shouldly;
using System;
using System.Text;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Sandbox1.Tests
{
    public class RoundtripTests
    {
        [Fact]

        public async Task Generic_MyTree_OutputVerify()
        {
            var orig = new MyOrg.Models.JsonSystemText.MyTree()
            {
                Count = 1,
                Key = "abc",
                Value = Encoding.UTF8.GetBytes("The quick brown fox jumps over the lazy dog."),
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.MyTree>();

            await Verifier.Verify(buffer);
        }

        [Fact]
        public void Generic_MyTree_Roundtrip_NoRecurse()
        {
            var orig = new MyOrg.Models.JsonSystemText.MyTree()
            {
                Count = 1,
                Key = "abc",
                Value = Encoding.UTF8.GetBytes("Value for 'abc'."),
                // todo recurse
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.MyTree>();

            var copy = buffer.DeserializeFromJson<MyOrg.Models.JsonSystemText.MyTree>();
            copy.ShouldNotBeNull();
            copy.Freeze();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Count == orig.Count).ShouldBeTrue();
            (copy.Key == orig.Key).ShouldBeTrue();
            copy.Value.AsSpan().SequenceEqual(orig.Value.AsSpan()).ShouldBeTrue();
        }

        [Fact]
        public void Generic_MyTree_Roundtrip_Recurse()
        {
            var left = new MyOrg.Models.JsonSystemText.MyTree()
            {
                Count = 1,
                Key = "a",
                Value = Encoding.UTF8.GetBytes("Value for 'a'."),
            };
            var right = new MyOrg.Models.JsonSystemText.MyTree()
            {
                Count = 1,
                Key = "c",
                Value = Encoding.UTF8.GetBytes("Value for 'c'."),
            };
            var orig = new MyOrg.Models.JsonSystemText.MyTree()
            {
                Count = 1,
                Key = "b",
                Value = Encoding.UTF8.GetBytes("Value for 'b'."),
                Left = left,
                Right = right,
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.MyTree>();

            var copy = buffer.DeserializeFromJson<MyOrg.Models.JsonSystemText.MyTree>();
            copy.ShouldNotBeNull();
            copy.Freeze();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Count == orig.Count).ShouldBeTrue();
            (copy.Key == orig.Key).ShouldBeTrue();
            copy.Value.AsSpan().SequenceEqual(orig.Value.AsSpan()).ShouldBeTrue();
        }

    }
}