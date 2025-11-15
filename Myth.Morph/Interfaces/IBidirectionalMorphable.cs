namespace Myth.Interfaces;

/// <summary>
/// Interface combinada para objetos que suportam transformação bidirecional.
/// Use quando há necessidade de mapeamento nos dois sentidos.
/// </summary>
/// <typeparam name="TSource">The source type for bidirectional transformation.</typeparam>
/// <typeparam name="TDestination">The destination type for bidirectional transformation.</typeparam>
public interface IBidirectionalMorphable<TSource, TDestination>
	: IMorphableTo<TDestination>, IMorphableFrom<TSource> {
}
