using System.Collections.Generic;
using Myth.Flow.Test.Interfaces;

public class MockRoleValidator : IRoleValidator {
	private readonly HashSet<string> _validRoles = new( ) { "admin", "common", "maintainer" };

	public bool IsValidRole( string role ) => _validRoles.Contains( role.ToLower( ) );
}
