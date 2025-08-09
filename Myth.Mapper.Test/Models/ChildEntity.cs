using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class ChildEntity : IMapTo<ChildEntity, ChildDto> {
		public int Id { get; set; }
		public ParentEntity? Parent { get; set; }

		public void MapTo( MappingBuilder<ChildEntity, ChildDto> builder ) {
			builder.ForMember(
				dest => dest.ParentId,
				src => src.Parent != null ? src.Parent.Id : 0 );
		}
	}
}