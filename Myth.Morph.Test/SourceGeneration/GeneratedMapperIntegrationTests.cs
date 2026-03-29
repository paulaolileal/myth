using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph;
using Myth.ServiceProvider;

namespace Myth.Morph.Test.SourceGeneration;

/// <summary>
/// Integration tests verifying that <see cref="BaseMappingExecutor{TDestination}"/>
/// uses a registered <see cref="IMorphAutoMapper"/> instead of reflection when one is available.
/// This is the core contract of the source generation fast path.
/// </summary>
[Collection( "Sequential" )]
public sealed class GeneratedMapperIntegrationTests : IDisposable {

	private readonly IServiceProvider _serviceProvider;

	public GeneratedMapperIntegrationTests( ) {
		var services = new ServiceCollection( );
		services.AddLogging( );
		services.AddMorph( );

		_serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( _serviceProvider );

		// Register the spy auto-mapper before any mapping call
		MorphAutoMapperRegistry.Register( new SpyAutoMapper( ) );
	}

	public void Dispose( ) {
		MorphAutoMapperRegistry.Clear( );
		MythServiceProvider.Reset( );
		( _serviceProvider as IDisposable )?.Dispose( );
	}

	// ─── Fast path activation ────────────────────────────────────────────────────

	[Fact]
	public void To_ShouldUse_RegisteredAutoMapper_InsteadOfReflection( ) {
		var source = new GeneratedMapperSource { Id = 42, Name = "Alice" };

		var result = source.To<GeneratedMapperDestination>( _serviceProvider );

		SpyAutoMapper.CallCount.Should( ).BeGreaterThan( 0, "the generated auto-mapper should have been invoked" );
	}

	[Fact]
	public void To_ShouldProduceCorrectResult_WhenAutoMapperIsRegistered( ) {
		var source = new GeneratedMapperSource { Id = 7, Name = "Bob" };

		var result = source.To<GeneratedMapperDestination>( _serviceProvider );

		result.Should( ).NotBeNull( );
		result.Id.Should( ).Be( 7 );
		result.Name.Should( ).Be( "Bob" );
	}

	[Fact]
	public void AutoMapper_ShouldRespect_SkipSet_ForManuallyBoundProperties( ) {
		var source = new GeneratedMapperSource { Id = 99, Name = "Skipped" };

		SpyAutoMapper.LastSkipSet = null;
		source.To<GeneratedMapperDestination>( _serviceProvider );

		SpyAutoMapper.LastSkipSet.Should( ).NotBeNull( );
	}

	// ─── Test helpers ────────────────────────────────────────────────────────────

	private sealed class GeneratedMapperSource : IMorphableTo<GeneratedMapperDestination> {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;

		public void MorphTo( Schema<GeneratedMapperDestination> schema ) {
			// No manual bindings — everything goes through AutoMapProperties
		}
	}

	private sealed class GeneratedMapperDestination {
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	/// <summary>
	/// Spy implementation of <see cref="IMorphAutoMapper"/> that:
	/// <list type="bullet">
	///   <item>Records how many times it was called.</item>
	///   <item>Applies the property assignments itself (validates real behavior).</item>
	///   <item>Exposes the last received <c>skip</c> set for assertion.</item>
	/// </list>
	/// </summary>
	private sealed class SpyAutoMapper : IMorphAutoMapper {
		public static int CallCount;
		public static HashSet<string>? LastSkipSet;

		public Type SourceType => typeof( GeneratedMapperSource );
		public Type DestinationType => typeof( GeneratedMapperDestination );

		public void MapProperties( object source, object destination, HashSet<string> skip ) {
			CallCount++;
			LastSkipSet = skip;

			var src = ( GeneratedMapperSource )source;
			var dest = ( GeneratedMapperDestination )destination;

			if ( !skip.Contains( "Id" ) )
				dest.Id = src.Id;
			if ( !skip.Contains( "Name" ) )
				dest.Name = src.Name;
		}
	}
}
