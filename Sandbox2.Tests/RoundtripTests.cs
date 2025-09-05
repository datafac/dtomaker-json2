using DTOMaker.Runtime.JsonSystemText;
using Shouldly;
using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Sandbox2.Tests
{
    public class RoundtripTests
    {
        [Fact]
        public async Task Generic_Rectangle_OutputVerify_AsLeaf()
        {
            var orig = new MyOrg.Models.JsonSystemText.Rectangle()
            {
                Length = 123.0,
                Height = 456.0,
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.Rectangle>();

            await Verifier.Verify(buffer);
        }

        [Fact]
        public async Task Generic_Rectangle_OutputVerify_AsRoot()
        {
            var orig = new MyOrg.Models.JsonSystemText.Rectangle()
            {
                Length = 123.0,
                Height = 456.0,
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.Polygon>();

            await Verifier.Verify(buffer);
        }

        [Fact]
        public void Generic_Rectangle_Roundtrip_AsLeaf()
        {
            var orig = new MyOrg.Models.JsonSystemText.Rectangle()
            {
                Length = 123.0,
                Height = 456.0,
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.Rectangle>();

            var copy = buffer.DeserializeFromJson<MyOrg.Models.JsonSystemText.Rectangle>();
            copy.ShouldNotBeNull();
            copy.Freeze();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Length == orig.Length).ShouldBeTrue();
            (copy.Height == orig.Height).ShouldBeTrue();
        }

        [Fact]
        public void Generic_Rectangle_Roundtrip_AsRoot()
        {
            var orig = new MyOrg.Models.JsonSystemText.Rectangle()
            {
                Length = 123.0,
                Height = 456.0,
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.Polygon>();
            var recd = buffer.DeserializeFromJson<MyOrg.Models.JsonSystemText.Polygon>();
            recd.ShouldNotBeNull();
            recd.Freeze();

            recd.ShouldBeOfType<MyOrg.Models.JsonSystemText.Rectangle>();
            var copy = recd as MyOrg.Models.JsonSystemText.Rectangle ?? throw new InvalidCastException();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Length == orig.Length).ShouldBeTrue();
            (copy.Height == orig.Height).ShouldBeTrue();
        }

    }
}