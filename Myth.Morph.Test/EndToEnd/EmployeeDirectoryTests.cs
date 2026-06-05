using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;

namespace Myth.Morph.Test.EndToEnd;

/// <summary>
/// End-to-end tests for the employee directory domain.
///
/// Patterns covered:
/// <list type="bullet">
///   <item><see cref="Employee"/> : IMorphableTo&lt;EmployeeCardDto&gt; — entity drives the projection</item>
///   <item>Computed fields: FullName (concat), YearsAtCompany (date math), StatusBadge (conditional)</item>
///   <item>Async binding with DI service: PhotoUrl resolved via IEmployeePhotoService</item>
///   <item>Auto-mapped fields: Id, Email, Department, JobTitle, ManagerName</item>
///   <item>Nullable field handling: PhotoUrl and ManagerName can be null</item>
///   <item>Collection async mapping: list of employees → list of cards</item>
/// </list>
/// </summary>
public sealed class EmployeeDirectoryTests : BaseTestFixture {

	// ─── Fixture setup ────────────────────────────────────────────────────────────

	private static readonly Dictionary<int, string> PhotoRegistry = new( ) {
		[ 1 ] = "https://cdn.example.com/photos/1.jpg",
		[ 2 ] = "https://cdn.example.com/photos/2.jpg",
	};

	protected override void ConfigureServices( IServiceCollection services ) {
		services.AddSingleton<IEmployeePhotoService>(
			new FakeEmployeePhotoService( PhotoRegistry ) );
	}

	private static Employee BuildEmployee(
		int id = 1,
		string firstName = "Alice",
		string lastName = "Smith",
		bool isActive = true,
		string? managerName = null,
		int yearsAgo = 3 ) =>
		new( ) {
			Id = id,
			FirstName = firstName,
			LastName = lastName,
			Email = $"{firstName.ToLower( )}.{lastName.ToLower( )}@company.com",
			Department = "Engineering",
			JobTitle = "Senior Developer",
			HiredAt = DateTime.UtcNow.AddYears( -yearsAgo ).AddDays( -30 ),
			IsActive = isActive,
			ManagerName = managerName
		};

	// ─── Auto-mapped fields ───────────────────────────────────────────────────────

	[Fact]
	public async Task Employee_To_EmployeeCardDto_ShouldAutoMap_IdEmailDepartmentJobTitle( ) {
		var employee = BuildEmployee( id: 42, firstName: "Bob", lastName: "Jones" );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.Id.Should( ).Be( 42 );
		card.Email.Should( ).Be( "bob.jones@company.com" );
		card.Department.Should( ).Be( "Engineering" );
		card.JobTitle.Should( ).Be( "Senior Developer" );
	}

	// ─── Computed fields ─────────────────────────────────────────────────────────

	[Fact]
	public async Task Employee_To_EmployeeCardDto_ShouldCompute_FullName( ) {
		var employee = BuildEmployee( firstName: "Alice", lastName: "Wonderland" );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.FullName.Should( ).Be( "Alice Wonderland" );
	}

	[Fact]
	public async Task Employee_Active_ShouldHave_ActiveStatusBadge( ) {
		var employee = BuildEmployee( isActive: true );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.StatusBadge.Should( ).Be( "Active" );
	}

	[Fact]
	public async Task Employee_Inactive_ShouldHave_InactiveStatusBadge( ) {
		var employee = BuildEmployee( isActive: false );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.StatusBadge.Should( ).Be( "Inactive" );
	}

	[Fact]
	public async Task Employee_HiredThreeYearsAgo_ShouldHave_CorrectYearsAtCompany( ) {
		var employee = BuildEmployee( yearsAgo: 3 );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		// AddDays(-30) puts them just past the 3-year mark
		card.YearsAtCompany.Should( ).Be( 3 );
	}

	[Fact]
	public async Task Employee_HiredJustYesterday_ShouldHave_ZeroYearsAtCompany( ) {
		var employee = BuildEmployee( yearsAgo: 0 );
		employee.HiredAt = DateTime.UtcNow.AddDays( -1 );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.YearsAtCompany.Should( ).Be( 0 );
	}

	// ─── Async binding with DI service ───────────────────────────────────────────

	[Fact]
	public async Task Employee_WithPhotoRegistered_ShouldResolve_PhotoUrl( ) {
		var employee = BuildEmployee( id: 1 );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.PhotoUrl.Should( ).Be( "https://cdn.example.com/photos/1.jpg" );
	}

	[Fact]
	public async Task Employee_WithoutPhotoRegistered_ShouldHave_NullPhotoUrl( ) {
		var employee = BuildEmployee( id: 999 ); // no photo in registry

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.PhotoUrl.Should( ).BeNull( );
	}

	// ─── Nullable field propagation ───────────────────────────────────────────────

	[Fact]
	public async Task Employee_WithManager_ShouldAutoMap_ManagerName( ) {
		var employee = BuildEmployee( managerName: "Carol Boss" );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.ManagerName.Should( ).Be( "Carol Boss" );
	}

	[Fact]
	public async Task Employee_WithoutManager_ShouldHave_NullManagerName( ) {
		var employee = BuildEmployee( managerName: null );

		var card = await employee.ToAsync<EmployeeCardDto>( ServiceProvider );

		card.ManagerName.Should( ).BeNull( );
	}

	// ─── Collection mapping ───────────────────────────────────────────────────────

	[Fact]
	public async Task ListOfEmployees_ToAsync_ShouldMapAll_WithCorrectFullNames( ) {
		var employees = new List<Employee> {
			BuildEmployee( id: 1, firstName: "Alice", lastName: "Smith" ),
			BuildEmployee( id: 2, firstName: "Bob",   lastName: "Jones" ),
			BuildEmployee( id: 3, firstName: "Carol", lastName: "White", isActive: false ),
		};

		var cards = ( await employees.ToAsync<EmployeeCardDto>( ServiceProvider ) ).ToList( );

		cards.Should( ).HaveCount( 3 );
		cards[ 0 ].FullName.Should( ).Be( "Alice Smith" );
		cards[ 1 ].FullName.Should( ).Be( "Bob Jones" );
		cards[ 2 ].FullName.Should( ).Be( "Carol White" );
		cards[ 2 ].StatusBadge.Should( ).Be( "Inactive" );
	}

	[Fact]
	public async Task ListOfEmployees_ToAsync_ShouldResolve_PhotoUrlForEachEmployee( ) {
		var employees = new List<Employee> {
			BuildEmployee( id: 1 ),
			BuildEmployee( id: 2 ),
			BuildEmployee( id: 999 ),
		};

		var cards = ( await employees.ToAsync<EmployeeCardDto>( ServiceProvider ) ).ToList( );

		cards[ 0 ].PhotoUrl.Should( ).Be( "https://cdn.example.com/photos/1.jpg" );
		cards[ 1 ].PhotoUrl.Should( ).Be( "https://cdn.example.com/photos/2.jpg" );
		cards[ 2 ].PhotoUrl.Should( ).BeNull( );
	}

	[Fact]
	public async Task EmptyEmployeeList_ToAsync_ShouldReturnEmpty( ) {
		var cards = await new List<Employee>( ).ToAsync<EmployeeCardDto>( ServiceProvider );
		cards.Should( ).BeEmpty( );
	}

	// ─── CanBindTo ────────────────────────────────────────────────────────────────

	[Fact]
	public void Employee_CanBindTo_EmployeeCardDto_ShouldBeTrue( ) {
		BuildEmployee( ).CanBindTo<EmployeeCardDto>( ServiceProvider ).Should( ).BeTrue( );
	}

	[Fact]
	public void Employee_CanBindTo_UnregisteredType_ShouldBeFalse( ) {
		BuildEmployee( ).CanBindTo<string>( ServiceProvider ).Should( ).BeFalse( );
	}
}
