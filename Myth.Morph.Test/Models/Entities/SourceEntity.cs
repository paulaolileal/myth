using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class SourceEntity : IMorphableTo<DestEntity> {
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string IgnoredValue { get; set; } = "";

		public void MorphTo( Schema<DestEntity> schema ) {
			schema.Ignore( dest => dest.IgnoredProperty );
		}
	}
}