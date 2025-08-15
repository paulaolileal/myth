using Myth.Mapper;

namespace Myth.Interfaces {

	public interface IMapTo<TDestination> {

		/// <summary>
		/// Configura o mapeamento da instância atual para o tipo de destino
		/// </summary>
		/// <param name="builder">Builder para configurar o mapeamento</param>
		void MapTo( MappingBuilder<TDestination> builder );
	}
}