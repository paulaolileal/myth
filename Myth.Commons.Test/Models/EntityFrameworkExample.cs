using Myth.ValueObjects;

namespace Myth.Commons.Test.Models;

/// <summary>
/// Example of how the new Constant works with Entity Framework
/// </summary>
public class OrderStatus( string name, string value ) : Constant<OrderStatus, string>( name, value ) {
	public static readonly OrderStatus Pending = new( name: "Pending", value: "P" );
	public static readonly OrderStatus Processing = new( name: "Processing", value: "PR" );
	public static readonly OrderStatus Completed = new( name: "Completed", value: "C" );
	public static readonly OrderStatus Cancelled = new( name: "Cancelled", value: "X" );
}

/// <summary>
/// Example entity that uses OrderStatus
/// </summary>
public class Order {
	public int Id { get; set; }
	public string CustomerName { get; set; } = string.Empty;
	public OrderStatus Status { get; set; } = OrderStatus.Pending;
	public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Example Entity Framework configuration
/// This demonstrates how HasConversion continues to work perfectly
/// </summary>
public static class EntityFrameworkConfiguration {
	/*
	// In your DbContext OnModelCreating method:
	public void Configure(EntityTypeBuilder<Order> builder) {
		builder
			.Property(x => x.Status)
			.HasColumnName("status")
			.HasConversion(
				// Convert to database: Constant -> Value (string "P", "PR", etc.)
				status => status.Value,
				// Convert from database: Value -> Constant
				value => OrderStatus.FromValue(value)
			)
			.IsRequired();
	}
	*/

	/// <summary>
	/// Alternative configuration using the simplified syntax
	/// </summary>
	/*
	public void ConfigureSimplified(EntityTypeBuilder<Order> builder) {
		builder
			.Property(x => x.Status)
			.HasConversion<string>(
				// The implicit conversion to TValue works automatically
				status => status,  // Uses implicit operator TValue
				// FromValue continues to work as before
				value => OrderStatus.FromValue(value)
			);
	}
	*/
}

/// <summary>
/// Example demonstrating all the DX improvements
/// </summary>
public static class UsageExamples {
	public static void DemonstrateNewSyntax( ) {
		// ✅ Simplified syntax - no explicit generics needed!
		_ = OrderStatus.GetAll( );           // Instead of Constant.GetAll<OrderStatus>()

		_ = OrderStatus.GetOptions( );   // Instead of Constant.GetOptions<OrderStatus>()
		var status = OrderStatus.FromValue( "P" ); // Instead of Constant.FromValue<OrderStatus>("P")

		// ✅ Easy iteration for switch statements
		foreach ( var orderStatus in OrderStatus.All ) {
			// Process each status
		}

		// ✅ Switch expressions work beautifully
		_ = status.Value switch {
			"P" => "Order is pending",
			"PR" => "Order is being processed",
			"C" => "Order completed",
			"X" => "Order cancelled",
			_ => "Unknown status"
		};


		// ✅ Pattern matching with constants
		_ = status switch {
			var s when s == OrderStatus.Pending => "Waiting for processing",
			var s when s == OrderStatus.Processing => "Currently being handled",
			var s when s == OrderStatus.Completed => "All done!",
			var s when s == OrderStatus.Cancelled => "Order was cancelled",
			_ => "Unknown"
		};


		// ✅ Values class for easy pattern matching
		_ = OrderStatus.Values.All.Contains( status.Value );


		// ✅ Implicit conversion to Value works great
		_ = status;     // Gets the Value via implicit conversion

		_ = status.Name; // Gets the Name via property

		// ✅ Try methods for safe access
		if ( OrderStatus.TryFromValue( "P", out var found ) ) {
			Console.WriteLine( $"Found: {found.Name}" );
		}

		// ✅ Entity Framework compatibility maintained
		_ = new Order {
			Status = OrderStatus.Pending,  // Assigns directly
			CustomerName = "John Doe"
		};

		// The HasConversion will work exactly as before:
		// status => status.Value        (implicit conversion)
		// value => OrderStatus.FromValue(value)  (static method)
	}
}
