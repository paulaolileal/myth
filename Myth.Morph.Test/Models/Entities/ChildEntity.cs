using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models;

public class ChildEntity : IMorphableTo<ChildDto> {
	public int Id { get; set; }
	public ParentEntity? Parent { get; set; }

	public void MorphTo( Schema<ChildDto> schema ) {
		schema.Bind(
			dest => dest.ParentId,
			( ) => Parent != null ? Parent.Id : 0 );
	}
}
