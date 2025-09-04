using System;
using DataFac.Memory;
using DTOMaker.Models;
using DTOMaker.Runtime;
namespace MyOrg.Models
{
    [Entity]
    [Id(1)]
    public interface ITree<TK, TV> : IEntityBase
    {
        [Member(1)] int Count { get; set; }
        [Member(2)] TK Key { get; set; }
        [Member(3)] TV Value { get; set; }
        [Member(4)] ITree<TK, TV>? Left { get; set; }
        [Member(5)] ITree<TK, TV>? Right { get; set; }
    }
    [Entity]
    [Id(2)]
    public interface IMyTree : ITree<String, Octets>
    {
    }
}
