using Myth.Morph;

namespace Myth.Interfaces {

	public interface IMorphTo<TDestination> {

		/// <summary>
		/// Configura o mapeamento da instância atual para o tipo de destino
		/// </summary>
		/// <param name="builder">Builder para configurar o mapeamento</param>
		void Binder( BinderBuilder<TDestination> builder );
	}
}