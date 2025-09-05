using System;
using DataFac.Memory;
using DTOMaker.Models;
using DTOMaker.Runtime;
namespace MyOrg.Models
{
    [Entity]
    [Id(1)]
    public interface IMyDTO : IEntityBase
    {
        [Member(1)] PairOfInt64 Custom1 { get; set; }
        [Member(2)] PairOfInt32 Custom2 { get; set; }
        [Member(3)] PairOfInt16 Custom3 { get; set; }
    }
}
