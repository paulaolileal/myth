using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class ParentEntity : IMapTo<ParentEntity, ParentDto> {
		public int Id { get; set; }
		public ChildEntity? Child { get; set; }

		public void MapTo( MappingBuilder<ParentEntity, ParentDto> builder ) {
			builder
				.ForMember(
					dest => dest.Child,
					src => src.Child.MapTo<ChildDto>( ) );
		}
	}
}