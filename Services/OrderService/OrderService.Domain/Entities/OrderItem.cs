namespace OrderService.Domain.Entities;

using OrderService.Domain.Common;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPriceSnapshot { get; set; }
    public string? CustomizationNotes { get; set; }
    public decimal Subtotal => decimal.Round(Quantity * UnitPriceSnapshot, 2, MidpointRounding.AwayFromZero);
}