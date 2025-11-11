using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;

namespace Myth.Morph.Test.Models;

public class EntityWithNested : IMorphableTo<DtoWithNested> {
	public int Id { get; set; }
	public List<NestedItem> Items { get; set; } = [ ];

	public void MorphTo( Schema<DtoWithNested> schema ) {
	}
}
