using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.Models {

	public class ChildEntity : IMorphTo<ChildDto> {
		public int Id { get; set; }
		public ParentEntity? Parent { get; set; }

		public void Binder( BinderBuilder<ChildDto> builder ) {
			builder.Bind(
				dest => dest.ParentId,
				( ) => Parent != null ? Parent.Id : 0 );
		}
	}
}