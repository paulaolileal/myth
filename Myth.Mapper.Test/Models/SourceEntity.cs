using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class SourceEntity : IMorphable<DestEntity> {
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string IgnoredValue { get; set; } = "";

		public void MorphTo( Schema<DestEntity> builder ) {
			builder.Ignore( dest => dest.IgnoredProperty );
		}
	}
}