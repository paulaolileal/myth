using FluentAssertions;
using Myth.Builder;
using Myth.Interfaces;
using Myth.Validation;

namespace Myth.Guard.Test;

/// <summary>
/// Tests for nullable struct validation rules (NotDefault)
/// </summary>
public class NullableStructRulesTests : BaseTestFixture {
	private readonly Validator _validator;

	public NullableStructRulesTests( ) {
		_validator = new Validator( ServiceProvider );
	}

	[Fact]
	public async Task NullableGuid_WhenNull_ShouldPass( ) {
		// Arrange
		var entity = new NullableGuidEntity { UserId = null };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task NullableGuid_WhenDefault_ShouldFail( ) {
		// Arrange
		var entity = new NullableGuidEntity { UserId = Guid.Empty };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "UserId" );
	}

	[Fact]
	public async Task NullableGuid_WhenValidValue_ShouldPass( ) {
		// Arrange
		var entity = new NullableGuidEntity { UserId = Guid.NewGuid( ) };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task NullableGuid_WhenDefault_ShouldUseCustomMessage( ) {
		// Arrange
		var entity = new NullableGuidEntityWithCustomMessage { UserId = Guid.Empty };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "UserId" && e.Message == "Custom message...." );
	}

	[Fact]
	public async Task NullableInt_WhenNull_ShouldPass( ) {
		// Arrange
		var entity = new NullableIntEntity { Score = null };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}

	[Fact]
	public async Task NullableInt_WhenDefault_ShouldFail( ) {
		// Arrange
		var entity = new NullableIntEntity { Score = 0 };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeFalse( );
		result.Errors.Should( ).Contain( e => e.Field == "Score" );
	}

	[Fact]
	public async Task NullableInt_WhenNonDefault_ShouldPass( ) {
		// Arrange
		var entity = new NullableIntEntity { Score = 42 };

		// Act
		var result = await _validator.ValidateAndReturnAsync( entity );

		// Assert
		result.IsValid.Should( ).BeTrue( );
	}
}

internal class NullableGuidEntity : IValidatable<NullableGuidEntity> {
	public Guid? UserId { get; set; }

	public void Validate( ValidationBuilder<NullableGuidEntity> builder, ValidationContextKey? context = null ) {
		builder.For( UserId, rules => rules.NotDefault( ) );
	}
}

internal class NullableGuidEntityWithCustomMessage : IValidatable<NullableGuidEntityWithCustomMessage> {
	public Guid? UserId { get; set; }

	public void Validate( ValidationBuilder<NullableGuidEntityWithCustomMessage> builder, ValidationContextKey? context = null ) {
		builder.For( UserId, rules => rules.NotDefault( ).WithMessage( "Custom message...." ) );
	}
}

internal class NullableIntEntity : IValidatable<NullableIntEntity> {
	public int? Score { get; set; }

	public void Validate( ValidationBuilder<NullableIntEntity> builder, ValidationContextKey? context = null ) {
		builder.For( Score, rules => rules.NotDefault( ) );
	}
}
