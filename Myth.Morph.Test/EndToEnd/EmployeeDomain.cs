using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.EndToEnd;

// ─── Entities ────────────────────────────────────────────────────────────────

/// <summary>
/// Employee entity. Implements IMorphableTo to control how it projects
/// itself to the card DTO, including an async call to the photo service.
/// </summary>
internal class Employee : IMorphableTo<EmployeeCardDto> {
	public int Id { get; set; }
	public string FirstName { get; set; } = "";
	public string LastName { get; set; } = "";
	public string Email { get; set; } = "";
	public string Department { get; set; } = "";
	public string JobTitle { get; set; } = "";
	public DateTime HiredAt { get; set; }
	public bool IsActive { get; set; }
	public string? ManagerName { get; set; }

	public void MorphTo( Schema<EmployeeCardDto> schema ) {
		schema
			.Bind( dest => dest.FullName, sp => $"{FirstName} {LastName}" )
			.Bind( dest => dest.YearsAtCompany, sp => CalculateYearsAtCompany( ) )
			.Bind( dest => dest.StatusBadge, sp => IsActive ? "Active" : "Inactive" )
			.BindAsync( dest => dest.PhotoUrl, async sp => {
				var service = sp.GetRequiredService<IEmployeePhotoService>( );
				return await service.GetPhotoUrlAsync( Id );
			} );
	}

	private int CalculateYearsAtCompany( ) =>
		( int )Math.Floor( ( DateTime.UtcNow - HiredAt ).TotalDays / 365.25 );
}

// ─── Services ────────────────────────────────────────────────────────────────

internal interface IEmployeePhotoService {
	Task<string?> GetPhotoUrlAsync( int employeeId );
}

internal sealed class FakeEmployeePhotoService( Dictionary<int, string>? photos = null ) : IEmployeePhotoService {
	private readonly Dictionary<int, string> _photos = photos ?? [ ];

	public Task<string?> GetPhotoUrlAsync( int employeeId ) {
		_photos.TryGetValue( employeeId, out var url );
		return Task.FromResult<string?>( url );
	}
}

// ─── DTOs ────────────────────────────────────────────────────────────────────

internal class EmployeeCardDto {
	public int Id { get; set; }
	public string FullName { get; set; } = "";
	public string Email { get; set; } = "";
	public string Department { get; set; } = "";
	public string JobTitle { get; set; } = "";
	public int YearsAtCompany { get; set; }
	public string StatusBadge { get; set; } = "";
	public string? PhotoUrl { get; set; }
	public string? ManagerName { get; set; }
}
