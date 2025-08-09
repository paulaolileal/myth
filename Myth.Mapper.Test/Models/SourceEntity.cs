using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class SourceEntity : IMapTo<SourceEntity, DestEntity> {
		public int Id { get; set; }
		public string Name { get; set; } = "";
		public string IgnoredValue { get; set; } = "";

		public void MapTo( MappingBuilder<SourceEntity, DestEntity> builder ) {
			builder.Ignore( dest => dest.IgnoredProperty );
		}
	}
}