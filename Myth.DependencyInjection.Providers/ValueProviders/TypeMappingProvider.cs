using AutoMapper;
using Myth.Exceptions;

namespace Myth.ValueProviders {

	public static class TypeMappingProvider {
		private static IMapper? _mapper;

		/// <summary>
		/// Make a property dependecy injection to be available in all project
		/// </summary>
		/// <param name="mapper"><c>AutoMapper</c> instance</param>
		public static void Configure( IMapper mapper ) {
			_mapper = mapper;
		}

		/// <summary>
		/// Map using AutoMapper the current object to destination type
		/// </summary>
		/// <typeparam name="TDest">The destination type</typeparam>
		/// <param name="model">The source object to be mapped</param>
		/// <returns>A object casted to TDest type</returns>
		/// <remarks>
		/// The AutoMapper profile must be created
		/// </remarks>
		public static TDest MapTo<TDest>( this object model ) =>
			_mapper is not null ? _mapper.Map<TDest>( model ) : throw new TypeMappingNotConfiguredException( );

		/// <summary>
		/// Map using AutoMapper the current object to destination type
		/// </summary>
		/// <typeparam name="TDest">The destination type</typeparam>
		/// <param name="model">The source object to be mapped</param>
		/// <returns>A object casted to TDest type</returns>
		/// <remarks>
		/// The AutoMapper profile must be created
		/// </remarks>
		public static async Task<TDest> MapToAsync<TSource, TDest>( this Task<TSource> task ) =>
			_mapper is not null ? _mapper.Map<TDest>( await task ) : throw new TypeMappingNotConfiguredException( );

		/// <summary>
		/// Map using AutoMapper the current object to destination type
		/// </summary>
		/// <typeparam name="TDest">The destination type</typeparam>
		/// <param name="model">The source object to be mapped</param>
		/// <returns>A object casted to TDest type</returns>
		/// <remarks>
		/// The AutoMapper profile must be created
		/// </remarks>
		public static async ValueTask<TDest> MapToAsync<TSource, TDest>( this ValueTask<TSource> task ) =>
			_mapper is not null ? _mapper.Map<TDest>( await task ) : throw new TypeMappingNotConfiguredException( );
	}
}