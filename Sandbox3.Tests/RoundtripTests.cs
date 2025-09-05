using DTOMaker.Runtime.JsonSystemText;
using Shouldly;
using System;
using System.Threading.Tasks;
using VerifyXunit;
using Xunit;

namespace Sandbox3.Tests
{
    public class RoundtripTests
    {
        [Fact]
        public async Task MyDTO_OutputVerify()
        {
            var orig = new MyOrg.Models.JsonSystemText.MyDTO()
            {
                Custom1 = new DataFac.Memory.PairOfInt64(1, 2),
                Custom2 = new DataFac.Memory.PairOfInt32(3, 4),
                Custom3 = new DataFac.Memory.PairOfInt16(5, 6),
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.MyDTO>();

            await Verifier.Verify(buffer);
        }

        [Fact]
        public void MyDTO_Roundtrip()
        {
            var orig = new MyOrg.Models.JsonSystemText.MyDTO()
            {
                Custom1 = new DataFac.Memory.PairOfInt64(1, 2),
                Custom2 = new DataFac.Memory.PairOfInt32(3, 4),
                Custom3 = new DataFac.Memory.PairOfInt16(5, 6),
            };
            orig.Freeze();

            var buffer = orig.SerializeToJson<MyOrg.Models.JsonSystemText.MyDTO>();

            var copy = buffer.DeserializeFromJson<MyOrg.Models.JsonSystemText.MyDTO>();
            copy.ShouldNotBeNull();
            copy.Freeze();

            copy.Equals(orig).ShouldBeTrue();
            (copy == orig).ShouldBeTrue();
            (copy.Custom1 == orig.Custom1).ShouldBeTrue();
            (copy.Custom2 == orig.Custom2).ShouldBeTrue();
            (copy.Custom3 == orig.Custom3).ShouldBeTrue();
        }
    }
}