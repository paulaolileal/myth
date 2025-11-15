using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models;

public class ParentEntity : IMorphableTo<ParentDto> {
	public int Id { get; set; }
	public ChildEntity? Child { get; set; }

	public void MorphTo( Schema<ParentDto> schema ) {
		schema
			.BindAsync(
				dest => dest.Child,
				async ( sp ) => Child.To<ChildDto>( sp ) );
	}
}
