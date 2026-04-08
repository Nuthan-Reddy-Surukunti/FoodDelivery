namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }

    public Cart? Cart { get; set; }

    public Guid MenuItemId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public string? CustomizationNotes { get; set; }

    public decimal Subtotal { get; set; }
}