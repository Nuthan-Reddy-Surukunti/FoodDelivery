namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal PriceSnapshot { get; set; }
    public string? CustomizationNotes { get; set; }
    public decimal Subtotal => decimal.Round(Quantity * PriceSnapshot, 2, MidpointRounding.AwayFromZero);
}