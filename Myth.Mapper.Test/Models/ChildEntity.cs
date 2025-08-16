using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class ChildEntity : IMorphable<ChildDto> {
		public int Id { get; set; }
		public ParentEntity? Parent { get; set; }

		public void MorphTo( Schema<ChildDto> builder ) {
			builder.Bind(
				dest => dest.ParentId,
				( ) => Parent != null ? Parent.Id : 0 );
		}
	}
}