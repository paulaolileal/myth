using Myth.Mapper;

namespace Myth.Interfaces {
    public interface IMapFrom<TSource, TDestination>
    {
        void MapFrom(MappingBuilder<TSource, TDestination> builder);
    }
}