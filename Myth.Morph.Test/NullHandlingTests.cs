using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph;
using Myth.ServiceProvider;
using Myth.Settings;

namespace Myth.Morph.Test;

// ─── Test models ─────────────────────────────────────────────────────────────

file class SourceForBindIfNotNull : IMorphableTo<DestForBindIfNotNull> {
	public string? Token { get; set; }
	public string? Role { get; set; }

	public void MorphTo( Schema<DestForBindIfNotNull> schema ) {
		schema
			.BindIfNotNull( dest => dest.Token, sp => Token )
			.BindIfNotNull( dest => dest.Role, sp => Role );
	}
}

file class DestForBindIfNotNull {
	public string Token { get; set; } = "initial-token";
	public string Role { get; set; } = "guest";
}

file class SourceForBindOrDefault : IMorphableTo<DestForBindOrDefault> {
	public string? DisplayName { get; set; }

	public void MorphTo( Schema<DestForBindOrDefault> schema ) {
		schema.BindOrDefault( dest => dest.DisplayName, sp => DisplayName, defaultValue: "Anonymous" );
	}
}

file class DestForBindOrDefault {
	public string DisplayName { get; set; } = "";
}

file class SourceForBindWhen : IMorphableTo<DestForBindWhen> {
	public string? AdminNotes { get; set; }
	public bool IsAdmin { get; set; }

	public void MorphTo( Schema<DestForBindWhen> schema ) {
		schema.BindWhen(
			dest => dest.AdminNotes,
			sp => AdminNotes,
			condition: sp => IsAdmin );
	}
}

file class DestForBindWhen {
	public string? AdminNotes { get; set; }
}

file class NullableAgeSource : IMorphableTo<NonNullableAgeDest> {
	public string? Name { get; set; }
	public int? Age { get; set; }

	public void MorphTo( Schema<NonNullableAgeDest> schema ) {
		// No manual binding for Age - auto-mapping will try to map int? (null) to int
	}
}

file class NonNullableAgeDest {
	public string? Name { get; set; }
	public int Age { get; set; }
}

// ─── BindIfNotNull tests ──────────────────────────────────────────────────────

public class BindIfNotNullTests : BaseTestFixture {

	[Fact]
	public void BindIfNotNull_Should_PreserveInitializedValue_WhenResolverReturnsNull( ) {
		// Arrange
		var source = new SourceForBindIfNotNull { Token = null, Role = null };

		// Act
		var dest = source.To<DestForBindIfNotNull>( ServiceProvider );

		// Assert
		dest.Should( ).NotBeNull( );
		dest.Token.Should( ).Be( "initial-token", "BindIfNotNull must preserve the initialized value when resolver returns null" );
		dest.Role.Should( ).Be( "guest", "BindIfNotNull must preserve the initialized value when resolver returns null" );
	}

	[Fact]
	public void BindIfNotNull_Should_AssignValue_WhenResolverReturnsNonNull( ) {
		// Arrange
		var source = new SourceForBindIfNotNull { Token = "abc-token", Role = "admin" };

		// Act
		var dest = source.To<DestForBindIfNotNull>( ServiceProvider );

		// Assert
		dest.Token.Should( ).Be( "abc-token" );
		dest.Role.Should( ).Be( "admin" );
	}

	[Fact]
	public void BindIfNotNull_Should_AssignNonNull_AndSkipNull_WhenMixed( ) {
		// Arrange
		var source = new SourceForBindIfNotNull { Token = "my-token", Role = null };

		// Act
		var dest = source.To<DestForBindIfNotNull>( ServiceProvider );

		// Assert
		dest.Token.Should( ).Be( "my-token" );
		dest.Role.Should( ).Be( "guest", "null role must keep initialized value" );
	}
}

// ─── BindOrDefault tests ──────────────────────────────────────────────────────

public class BindOrDefaultTests : BaseTestFixture {

	[Fact]
	public void BindOrDefault_Should_UseDefaultValue_WhenResolverReturnsNull( ) {
		// Arrange
		var source = new SourceForBindOrDefault { DisplayName = null };

		// Act
		var dest = source.To<DestForBindOrDefault>( ServiceProvider );

		// Assert
		dest.DisplayName.Should( ).Be( "Anonymous" );
	}

	[Fact]
	public void BindOrDefault_Should_UseResolvedValue_WhenResolverReturnsNonNull( ) {
		// Arrange
		var source = new SourceForBindOrDefault { DisplayName = "Paula" };

		// Act
		var dest = source.To<DestForBindOrDefault>( ServiceProvider );

		// Assert
		dest.DisplayName.Should( ).Be( "Paula" );
	}
}

// ─── BindWhen tests ───────────────────────────────────────────────────────────

public class BindWhenTests : BaseTestFixture {

	[Fact]
	public void BindWhen_Should_SkipBinding_WhenConditionIsFalse( ) {
		// Arrange
		var source = new SourceForBindWhen { AdminNotes = "Secret notes", IsAdmin = false };

		// Act
		var dest = source.To<DestForBindWhen>( ServiceProvider );

		// Assert
		dest.AdminNotes.Should( ).BeNull( "condition is false, binding must be skipped" );
	}

	[Fact]
	public void BindWhen_Should_ApplyBinding_WhenConditionIsTrue( ) {
		// Arrange
		var source = new SourceForBindWhen { AdminNotes = "Secret notes", IsAdmin = true };

		// Act
		var dest = source.To<DestForBindWhen>( ServiceProvider );

		// Assert
		dest.AdminNotes.Should( ).Be( "Secret notes" );
	}
}

// ─── NullPropertyBehavior.Skip tests ─────────────────────────────────────────

[Collection( "Sequential" )]
public class NullBehaviorSkipTests : IDisposable {

	public NullBehaviorSkipTests( ) {
		MythServiceProvider.Reset( );
		var services = new ServiceCollection( );
		services.AddLogging( );
		services.AddMorph( config => config.WithNullBehavior( NullPropertyBehavior.Skip ) );
		MythServiceProvider.Initialize( services.BuildServiceProvider( ) );
	}

	public void Dispose( ) => MythServiceProvider.Reset( );

	[Fact]
	public void NullBehavior_Skip_Should_PreserveInitializedValue_WhenSourcePropertyIsNull( ) {
		// Arrange
		var source = new SourceForBindIfNotNull { Token = null, Role = null };

		// Act — auto-mapping: null source props are skipped, initialized values preserved
		var dest = source.To<DestForBindIfNotNull>( );

		// Assert
		dest.Token.Should( ).Be( "initial-token", "Skip must preserve initialized value when source is null" );
		dest.Role.Should( ).Be( "guest", "Skip must preserve initialized value when source is null" );
	}

	[Fact]
	public void NullBehavior_Skip_Should_StillMapNonNullProperties( ) {
		// Arrange
		var source = new SourceForBindIfNotNull { Token = "valid-token", Role = null };

		// Act
		var dest = source.To<DestForBindIfNotNull>( );

		// Assert
		dest.Token.Should( ).Be( "valid-token" );
		dest.Role.Should( ).Be( "guest" );
	}
}

// ─── NullPropertyBehavior.Throw tests ────────────────────────────────────────

[Collection( "Sequential" )]
public class NullBehaviorThrowTests : IDisposable {

	public NullBehaviorThrowTests( ) {
		MythServiceProvider.Reset( );
		var services = new ServiceCollection( );
		services.AddLogging( );
		services.AddMorph( config => config.WithNullBehavior( NullPropertyBehavior.Throw ) );
		MythServiceProvider.Initialize( services.BuildServiceProvider( ) );
	}

	public void Dispose( ) => MythServiceProvider.Reset( );

	[Fact]
	public void NullBehavior_Throw_Should_ThrowMorphPropertyException_WhenNullMapsToNonNullableValueType( ) {
		// Arrange — int? = null mapped to int (non-nullable value type)
		var source = new NullableAgeSource { Name = "Test", Age = null };

		// Act
		var act = ( ) => source.To<NonNullableAgeDest>( );

		// Assert
		act.Should( ).Throw<MorphPropertyException>( )
			.Which.PropertyName.Should( ).Be( "Age" );
	}

	[Fact]
	public void NullBehavior_Throw_Should_IncludeSourceAndDestinationTypesInException( ) {
		// Arrange
		var source = new NullableAgeSource { Name = "Test", Age = null };

		// Act
		var act = ( ) => source.To<NonNullableAgeDest>( );

		// Assert
		act.Should( ).Throw<MorphPropertyException>( )
			.Which.DestinationType.Should( ).Be( typeof( NonNullableAgeDest ) );
	}

	[Fact]
	public void NullBehavior_Throw_Should_NotThrow_WhenAllPropertiesAreNonNull( ) {
		// Arrange
		var source = new NullableAgeSource { Name = "Test", Age = 25 };

		// Act
		var act = ( ) => source.To<NonNullableAgeDest>( );

		// Assert
		act.Should( ).NotThrow( );
	}
}

// ─── MorphPropertyException structure ────────────────────────────────────────

public class MorphPropertyExceptionTests {

	[Fact]
	public void MorphPropertyException_Create_Should_ContainAllContextFields( ) {
		// Arrange
		var inner = new InvalidOperationException( "inner error" );

		// Act
		var ex = MorphPropertyException.Create(
			typeof( SourceForBindIfNotNull ),
			typeof( DestForBindIfNotNull ),
			"Token",
			inner );

		// Assert
		ex.SourceType.Should( ).Be( typeof( SourceForBindIfNotNull ) );
		ex.DestinationType.Should( ).Be( typeof( DestForBindIfNotNull ) );
		ex.PropertyName.Should( ).Be( "Token" );
		ex.InnerException.Should( ).Be( inner );
		ex.Message.Should( ).Contain( "Token" );
		ex.Message.Should( ).Contain( "SourceForBindIfNotNull" );
		ex.Message.Should( ).Contain( "DestForBindIfNotNull" );
	}

	[Fact]
	public void MorphPropertyException_NullSourceValue_Should_DescribeNullViolation( ) {
		// Act
		var ex = MorphPropertyException.NullSourceValue(
			typeof( SourceForBindIfNotNull ),
			typeof( DestForBindIfNotNull ),
			"Token" );

		// Assert
		ex.PropertyName.Should( ).Be( "Token" );
		ex.Message.Should( ).Contain( "null" );
		ex.Message.Should( ).Contain( "Token" );
		ex.InnerException.Should( ).BeNull( );
	}
}
