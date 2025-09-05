using DTOMaker.Models;
using DTOMaker.Runtime;
namespace MyOrg.Models
{
    [Entity]
    [Id(1)]
    public interface IPolygon : IEntityBase { }

    [Entity]
    [Id(2)]
    public interface ITriangle : IPolygon { }

    [Entity]
    [Id(3)]
    public interface IEquilateral : ITriangle
    {
        [Member(1)] double Length { get; set; }
    }

    [Entity]
    [Id(4)]
    public interface IRightTriangle : ITriangle
    {
        [Member(1)] double Length { get; set; }
        [Member(2)] double Height { get; set; }
    }

    [Entity]
    [Id(5)]
    public interface IQuadrilateral : IPolygon { }

    [Entity]
    [Id(6)]
    public interface ISquare : IQuadrilateral
    {
        [Member(1)] double Length { get; set; }
    }

    [Entity]
    [Id(7)]
    public interface IRectangle : IQuadrilateral
    {
        [Member(1)] double Length { get; set; }
        [Member(2)] double Height { get; set; }
    }
}
