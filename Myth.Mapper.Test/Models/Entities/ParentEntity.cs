using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models {

	public class ParentEntity : IMorphable<ParentDto> {
		public int Id { get; set; }
		public ChildEntity? Child { get; set; }

		public void MorphTo( Schema<ParentDto> schema ) {
			schema
				.Bind(
					dest => dest.Child,
					( ) => Child.To<ChildDto>( ) );
		}
	}
}