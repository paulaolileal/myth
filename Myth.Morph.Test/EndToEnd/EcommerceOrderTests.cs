using FluentAssertions;
using Myth.Extensions;

namespace Myth.Morph.Test.EndToEnd;

/// <summary>
/// End-to-end tests for the e-commerce order domain.
///
/// Patterns covered:
/// <list type="bullet">
///   <item><see cref="Order"/> : IMorphableTo&lt;OrderDetailDto&gt; — entity drives the projection</item>
///   <item><see cref="OrderSummaryDto"/> : IMorphableFrom&lt;Order&gt; — DTO assembles itself from entity</item>
///   <item><see cref="OrderItemDto"/> : IMorphableFrom&lt;OrderItem&gt; — auto-mapping + one computed field</item>
///   <item>Collection mapping via <c>IEnumerable&lt;T&gt;.To&lt;TDest&gt;()</c></item>
///   <item>Async mapping via <c>ToAsync&lt;T&gt;()</c></item>
///   <item>Null source / empty collection handling</item>
/// </list>
/// </summary>
public sealed class EcommerceOrderTests : BaseTestFixture {

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static Order BuildOrder( OrderStatus status = OrderStatus.Processing ) =>
        new( ) {
            Id = 1001,
            OrderNumber = "ORD-2024-001",
            CustomerName = "Alice Wonderland",
            CustomerEmail = "alice@example.com",
            PlacedAt = new DateTime( 2024, 3, 15, 10, 30, 0, DateTimeKind.Utc ),
            Status = status,
            ShippingAddress = new ShippingAddress {
                Street = "123 Rabbit Hole",
                City = "Wonderland",
                Country = "GB",
                ZipCode = "WL1 1AA"
            },
            Items = [
                new OrderItem { Id = 1, ProductName = "Magic Mushroom", Quantity = 2, UnitPrice = 9.99m },
                new OrderItem { Id = 2, ProductName = "Rabbit Hat", Quantity = 1, UnitPrice = 49.99m },
                new OrderItem { Id = 3, ProductName = "Tea Set", Quantity = 3, UnitPrice = 15.00m },
            ]
        };

    // ─── IMorphableFrom: OrderSummaryDto ─────────────────────────────────────────

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldAutoMap_IdOrderNumberPlacedAtEmail( ) {
        var order = BuildOrder( );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.Id.Should( ).Be( 1001 );
        summary.OrderNumber.Should( ).Be( "ORD-2024-001" );
        summary.PlacedAt.Should( ).Be( order.PlacedAt );
        summary.CustomerEmail.Should( ).Be( "alice@example.com" );
    }

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldCompute_CustomerFromCustomerName( ) {
        var order = BuildOrder( );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.Customer.Should( ).Be( "Alice Wonderland" );
    }

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldCompute_TotalAmount( ) {
        var order = BuildOrder( );
        // (2 × 9.99) + (1 × 49.99) + (3 × 15.00) = 19.98 + 49.99 + 45.00 = 114.97
        var expected = 2 * 9.99m + 1 * 49.99m + 3 * 15.00m;

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.TotalAmount.Should( ).Be( expected );
    }

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldCompute_ItemCount( ) {
        var order = BuildOrder( );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.ItemCount.Should( ).Be( 3 );
    }

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldCompute_StatusLabel( ) {
        var order = BuildOrder( OrderStatus.Shipped );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.Status.Should( ).Be( "Shipped" );
    }

    [Fact]
    public void Order_To_OrderSummaryDto_ShouldCompute_ShippingTo_CityAndCountry( ) {
        var order = BuildOrder( );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.ShippingTo.Should( ).Be( "Wonderland, GB" );
    }

    [Fact]
    public void Order_With_EmptyItems_To_OrderSummaryDto_ShouldHaveZeroTotalAndZeroCount( ) {
        var order = BuildOrder( );
        order.Items.Clear( );

        var summary = order.To<OrderSummaryDto>( ServiceProvider );

        summary.TotalAmount.Should( ).Be( 0m );
        summary.ItemCount.Should( ).Be( 0 );
    }

    [Fact]
    public void Order_With_AllStatuses_ShouldMap_CorrectStatusLabel( ) {
        foreach ( var status in Enum.GetValues<OrderStatus>( ) ) {
            var order = BuildOrder( status );
            var summary = order.To<OrderSummaryDto>( ServiceProvider );
            summary.Status.Should( ).Be( status.ToString( ), because: $"status {status} should be mapped to its string name" );
        }
    }

    // ─── IMorphableFrom: OrderItemDto ────────────────────────────────────────────

    [Fact]
    public void OrderItem_To_OrderItemDto_ShouldAutoMap_AllBaseFields( ) {
        var item = new OrderItem { Id = 5, ProductName = "Widget", Quantity = 4, UnitPrice = 12.50m };

        var dto = item.To<OrderItemDto>( ServiceProvider );

        dto.Id.Should( ).Be( 5 );
        dto.ProductName.Should( ).Be( "Widget" );
        dto.Quantity.Should( ).Be( 4 );
        dto.UnitPrice.Should( ).Be( 12.50m );
    }

    [Fact]
    public void OrderItem_To_OrderItemDto_ShouldCompute_Subtotal( ) {
        var item = new OrderItem { Id = 1, ProductName = "Gizmo", Quantity = 3, UnitPrice = 7.25m };

        var dto = item.To<OrderItemDto>( ServiceProvider );

        dto.Subtotal.Should( ).Be( 3 * 7.25m );
    }

    // ─── Collection mapping ───────────────────────────────────────────────────────

    [Fact]
    public void ListOfOrders_To_OrderSummaryDto_ShouldMapAll( ) {
        var orders = new List<Order> {
            BuildOrder( OrderStatus.Pending ),
            BuildOrder( OrderStatus.Delivered ),
            BuildOrder( OrderStatus.Cancelled )
        };
        orders[ 1 ].Id = 2;
        orders[ 2 ].Id = 3;

        var summaries = orders.To<Order, OrderSummaryDto>( ServiceProvider ).ToList( );

        summaries.Should( ).HaveCount( 3 );
        summaries[ 0 ].Status.Should( ).Be( "Pending" );
        summaries[ 1 ].Status.Should( ).Be( "Delivered" );
        summaries[ 2 ].Status.Should( ).Be( "Cancelled" );
    }

    [Fact]
    public void EmptyOrderList_To_OrderSummaryDto_ShouldReturnEmpty( ) {
        var summaries = new List<Order>( ).To<Order, OrderSummaryDto>( ServiceProvider );
        summaries.Should( ).BeEmpty( );
    }

    // ─── Async mapping ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Order_ToAsync_OrderSummaryDto_ShouldMapCorrectly( ) {
        var order = BuildOrder( );

        var summary = await order.ToAsync<OrderSummaryDto>( ServiceProvider );

        summary.OrderNumber.Should( ).Be( "ORD-2024-001" );
        summary.ItemCount.Should( ).Be( 3 );
    }

    // ─── IMorphableTo: OrderDetailDto (includes async list mapping) ───────────────

    [Fact]
    public async Task Order_ToAsync_OrderDetailDto_ShouldMapComputedCustomerField( ) {
        var order = BuildOrder( );

        var detail = await order.ToAsync<OrderDetailDto>( ServiceProvider );

        detail.Customer.Should( ).Be( "Alice Wonderland <alice@example.com>" );
    }

    [Fact]
    public async Task Order_ToAsync_OrderDetailDto_ShouldCompute_Total( ) {
        var order = BuildOrder( );
        var expected = order.Items.Sum( i => i.UnitPrice * i.Quantity );

        var detail = await order.ToAsync<OrderDetailDto>( ServiceProvider );

        detail.Total.Should( ).Be( expected );
    }

    [Fact]
    public async Task Order_ToAsync_OrderDetailDto_ShouldInclude_MappedItems( ) {
        var order = BuildOrder( );

        var detail = await order.ToAsync<OrderDetailDto>( ServiceProvider );

        detail.Items.Should( ).HaveCount( 3 );
        detail.Items[ 0 ].ProductName.Should( ).Be( "Magic Mushroom" );
        detail.Items[ 0 ].Quantity.Should( ).Be( 2 );
        detail.Items[ 0 ].Subtotal.Should( ).Be( 2 * 9.99m );
        detail.Items[ 1 ].Subtotal.Should( ).Be( 1 * 49.99m );
        detail.Items[ 2 ].Subtotal.Should( ).Be( 3 * 15.00m );
    }

    [Fact]
    public async Task Order_ToAsync_OrderDetailDto_ShouldAutoMap_IdOrderNumberPlacedAt( ) {
        var order = BuildOrder( );

        var detail = await order.ToAsync<OrderDetailDto>( ServiceProvider );

        detail.Id.Should( ).Be( 1001 );
        detail.OrderNumber.Should( ).Be( "ORD-2024-001" );
        detail.PlacedAt.Should( ).Be( order.PlacedAt );
    }

    // ─── CanBindTo ────────────────────────────────────────────────────────────────

    [Fact]
    public void Order_CanBindTo_OrderDetailDto_ShouldBeTrue( ) {
        // Order : IMorphableTo<OrderDetailDto> — registered at startup via assembly scanning.
        var order = BuildOrder( );
        order.CanBindTo<OrderDetailDto>( ServiceProvider ).Should( ).BeTrue( );
    }

    [Fact]
    public void Order_CanBindTo_OrderSummaryDto_ReturnsFalse_BecauseIMorphableFromIsNotPreRegistered( ) {
        // OrderSummaryDto : IMorphableFrom<Order> — this pattern is discovered dynamically
        // inside SchemaRegistry.Morph() and is NOT added to the instance-based mappings set.
        // HasMapping() (and therefore CanBindTo()) only checks the pre-registered set,
        // so it correctly returns false for IMorphableFrom pairs.
        // The actual mapping DOES work — it is covered by the other To<>() tests above.
        var order = BuildOrder( );
        order.CanBindTo<OrderSummaryDto>( ServiceProvider ).Should( ).BeFalse( );
    }

    [Fact]
    public void Order_CanBindTo_UnregisteredType_ShouldBeFalse( ) {
        var order = BuildOrder( );
        order.CanBindTo<string>( ServiceProvider ).Should( ).BeFalse( );
    }
}
