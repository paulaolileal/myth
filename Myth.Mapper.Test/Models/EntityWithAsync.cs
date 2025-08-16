using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.Models {

	public class EntityWithAsync : IMorphTo<DtoWithAsync> {
		public int Id { get; set; }
		public Task<string> AsyncValue { get; set; } = Task.FromResult( "" );

		public void Binder( BinderBuilder<DtoWithAsync> builder ) {
			builder.BindAsync(
				dest => dest.Value,
				( ) => AsyncValue );
		}
	}
}