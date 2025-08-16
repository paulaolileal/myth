using Myth.Interfaces;

namespace Myth.Morph.Test.Models {

	public class EntityWithAsync : IMorphable<DtoWithAsync> {
		public int Id { get; set; }
		public Task<string> AsyncValue { get; set; } = Task.FromResult( "" );

		public void MorphTo( Schema<DtoWithAsync> builder ) {
			builder.BindAsync(
				dest => dest.Value,
				( ) => AsyncValue );
		}
	}
}