using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class EntityWithAsync : IMapTo<DtoWithAsync> {
		public int Id { get; set; }
		public Task<string> AsyncValue { get; set; } = Task.FromResult( "" );

		public void MapTo( MappingBuilder<DtoWithAsync> builder ) {
			builder.ForMemberAsync(
				dest => dest.Value,
				( ) => AsyncValue );
		}
	}
}