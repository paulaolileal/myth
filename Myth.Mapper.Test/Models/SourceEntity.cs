using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class SourceEntity : IMorphTo<DestEntity> {
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string IgnoredValue { get; set; } = "";

		public void Binder( BinderBuilder<DestEntity> builder ) {
			builder.Ignore( dest => dest.IgnoredProperty );
		}
	}
}