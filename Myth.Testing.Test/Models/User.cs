namespace Myth.Testing.Test.Models;

/// <summary>
/// Example user model
/// </summary>
public class User {

	/// <summary>
	/// Gets or sets the user ID
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Gets or sets the user name
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the user email
	/// </summary>
	public string Email { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the creation date
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	/// Gets or sets the last update date
	/// </summary>
	public DateTime? UpdatedAt { get; set; }
}
