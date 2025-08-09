using Myth.Interfaces;

namespace Myth.Mapper.Test.Models {

	public class EntityWithAsync : IMapTo<EntityWithAsync, DtoWithAsync> {
		public int Id { get; set; }
		public Task<string> AsyncValue { get; set; } = Task.FromResult( "" );

		public void MapTo( MappingBuilder<EntityWithAsync, DtoWithAsync> builder ) {
			builder.ForMemberAsync(
				dest => dest.Value,
				src => src.AsyncValue );
		}
	}
}