using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test.Models.Dtos;

/// <summary>
/// UserDto for testing Constant&lt;T,V&gt; to string mapping
/// </summary>
public record UserDto : IMorphableFrom<UserWithRole> {
	/// <summary>
	/// Gets or sets Id
	/// </summary>
	public Guid Id { get; init; }

	/// <summary>
	/// Gets or sets Name
	/// </summary>
	public string Name { get; init; } = null!;

	/// <summary>
	/// Gets or sets Email
	/// </summary>
	public string Email { get; init; } = null!;

	/// <summary>
	/// Gets or sets Avatar
	/// </summary>
	public string? Avatar { get; init; }

	/// <summary>
	/// Gets or sets Role as string
	/// </summary>
	public string Role { get; init; } = null!;

	public void MorphFrom( Schema<UserWithRole> schema ) {
		schema.Bind( ( ) => Id, src => src.UserId );
		schema.Bind( ( ) => Role, src => src.Role.Name );
	}
}
