using Myth.Extensions;
using Myth.Interfaces;
using Myth.Morph;

namespace Myth.Morph.Test.EndToEnd;

// ─── Entities ────────────────────────────────────────────────────────────────

/// <summary>
/// Order aggregate root. Implements IMorphableTo so the entity controls
/// how it maps to the full detail DTO.
/// </summary>
internal class Order : IMorphableTo<OrderDetailDto> {
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public DateTime PlacedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = [];
    public ShippingAddress ShippingAddress { get; set; } = new( );

    public void MorphTo( Schema<OrderDetailDto> schema ) {
        schema
            .Bind( dest => dest.Customer, sp => $"{CustomerName} <{CustomerEmail}>" )
            .Bind( dest => dest.Status, sp => Status.ToString( ) )
            .Bind( dest => dest.Total, sp => Items.Sum( i => i.UnitPrice * i.Quantity ) )
            .BindAsync( dest => dest.Items, async sp => Items.To<OrderItem, OrderItemDto>( sp ).ToList( ) );
    }
}

internal class OrderItem {
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

internal class ShippingAddress {
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

internal enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

// ─── DTOs ────────────────────────────────────────────────────────────────────

/// <summary>
/// Lightweight order summary. Uses IMorphableFrom so the DTO defines how it
/// assembles itself from the Order entity — useful when the entity is a pure domain
/// object that should not depend on DTO types.
/// </summary>
internal class OrderSummaryDto : IMorphableFrom<Order> {
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Customer { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public string Status { get; set; } = "";
    public DateTime PlacedAt { get; set; }
    public string ShippingTo { get; set; } = "";

    public void MorphFrom( Schema<Order> schema ) {
        schema
            .Bind( ( ) => Customer, src => src.CustomerName )
            .Bind( ( ) => TotalAmount, src => src.Items.Sum( i => i.UnitPrice * i.Quantity ) )
            .Bind( ( ) => ItemCount, src => src.Items.Count )
            .Bind( ( ) => Status, src => src.Status.ToString( ) )
            .Bind( ( ) => ShippingTo, src => $"{src.ShippingAddress.City}, {src.ShippingAddress.Country}" );
    }
}

/// <summary>
/// Per-item DTO. Mostly auto-mapped; adds one computed field (Subtotal).
/// </summary>
internal class OrderItemDto : IMorphableFrom<OrderItem> {
    public int Id { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }

    public void MorphFrom( Schema<OrderItem> schema ) {
        schema.Bind( ( ) => Subtotal, src => src.Quantity * src.UnitPrice );
    }
}

/// <summary>
/// Full order detail DTO — populated via Order.MorphTo().
/// </summary>
internal class OrderDetailDto {
    public int Id { get; set; }
    public string OrderNumber { get; set; } = "";
    public string Customer { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}
