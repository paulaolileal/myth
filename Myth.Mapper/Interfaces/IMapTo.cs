using Myth.Mapper;

namespace Myth.Interfaces {
    public interface IMapTo<TSource, TDestination>
    {
        void MapTo(MappingBuilder<TSource, TDestination> builder);
    }
}