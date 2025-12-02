namespace Myth.Morph.Test.Models.Entities;

/// <summary>
/// User entity for testing Constant&lt;T,V&gt; mapping
/// </summary>
public class UserWithRole {
	public Guid UserId { get; set; }
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? Avatar { get; set; }
	public UserRole Role { get; set; } = null!;
}
