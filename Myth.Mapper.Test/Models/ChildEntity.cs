using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class ChildEntity : IMapTo<ChildDto> {
		public int Id { get; set; }
		public ParentEntity? Parent { get; set; }

		public void MapTo( MappingBuilder<ChildDto> builder ) {
			builder.ForMember(
				dest => dest.ParentId,
				( ) => Parent != null ? Parent.Id : 0 );
		}
	}
}