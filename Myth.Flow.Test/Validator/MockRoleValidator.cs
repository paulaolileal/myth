using Myth.Flow.Test.Interfaces;
using System.Collections.Generic;

public class MockRoleValidator : IRoleValidator {
	private readonly HashSet<string> _validRoles = new( ) { "admin", "common", "maintainer" };

	public bool IsValidRole( string role ) => _validRoles.Contains( role.ToLower( ) );
}