using Myth.Extensions;
using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class ParentEntity : IMorphable<ParentDto> {
		public int Id { get; set; }
		public ChildEntity? Child { get; set; }

		public void MorphTo( Schema<ParentDto> builder ) {
			builder
				.Bind(
					dest => dest.Child,
					( ) => Child.To<ChildDto>( ) );
		}
	}
}