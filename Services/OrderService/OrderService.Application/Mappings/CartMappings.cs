namespace OrderService.Application.Mappings;

using OrderService.Application.DTOs.Cart;
using OrderService.Domain.Entities;

public static class CartMappings
{
    public static CartDto MapToDto(Cart cart)
    {
        var totalAmount = CalculateCartTotal(cart);

        return new CartDto
        {
            CartId = cart.Id,
            UserId = cart.UserId,
            RestaurantId = cart.RestaurantId,
            Status = cart.Status,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            TotalAmount = totalAmount,
            Currency = "USD",
            Items = cart.Items.Select(item => new CartItemDto
            {
                CartItemId = item.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                PriceSnapshot = item.Price,
                CustomizationNotes = item.CustomizationNotes,
                Subtotal = item.Quantity * item.Price
            }).ToList()
        };
    }

    private static decimal CalculateCartTotal(Cart cart)
    {
        return cart.Items.Sum(item => item.Quantity * item.Price);
    }
}
