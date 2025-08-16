using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.Models {

	public class ParentEntity : IMorphTo<ParentDto> {
		public int Id { get; set; }
		public ChildEntity? Child { get; set; }

		public void Binder( BinderBuilder<ParentDto> builder ) {
			builder
				.Bind(
					dest => dest.Child,
					( ) => Child.To<ChildDto>( ) );
		}
	}
}