using OrderService.Application.DTOs.Requests;
using OrderService.Application.Exceptions;
using OrderService.Application.Interfaces;
using OrderService.Application.Services;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;

namespace OrderService.Tests;

[TestFixture]
public class CartServiceTests
{
    private FakeCartRepository _cartRepository = null!;
    private FakeMenuItemValidationService _menuItemValidationService = null!;
    private CartService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _cartRepository = new FakeCartRepository();
        _menuItemValidationService = new FakeMenuItemValidationService();
        _service = new CartService(_cartRepository, _menuItemValidationService);
    }

    [Test]
    public async Task GetOrCreateCartAsync_WhenMissing_CreatesActiveCart()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        var result = await _service.GetOrCreateCartAsync(userId, restaurantId);

        Assert.Multiple(() =>
        {
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.RestaurantId, Is.EqualTo(restaurantId));
            Assert.That(result.Items, Is.Empty);
            Assert.That(_cartRepository.AddCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task AddCartItemAsync_WhenMenuItemValid_AddsItemAndUpdatesTotals()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        _menuItemValidationService.Result = new MenuItemValidationResult
        {
            IsValid = true,
            MenuItemId = menuItemId,
            CurrentPrice = 150
        };

        var result = await _service.AddCartItemAsync(new AddCartItemRequestDto
        {
            UserId = userId,
            RestaurantId = restaurantId,
            MenuItemId = menuItemId,
            Quantity = 2
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.TotalAmount, Is.EqualTo(300));
            Assert.That(_cartRepository.AddCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void AddCartItemAsync_WhenMenuItemInvalid_ThrowsValidationException()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        _menuItemValidationService.Result = new MenuItemValidationResult
        {
            IsValid = false,
            ErrorMessage = "Item unavailable"
        };

        Assert.ThrowsAsync<ValidationException>(async () =>
            await _service.AddCartItemAsync(new AddCartItemRequestDto
            {
                UserId = userId,
                RestaurantId = restaurantId,
                MenuItemId = Guid.NewGuid(),
                Quantity = 1
            }));
    }

    [Test]
    public async Task UpdateCartItemAsync_RecalculatesSubtotalAndCartTotal()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            TotalAmount = 200,
            Items =
            [
                new CartItem
                {
                    Id = Guid.NewGuid(),
                    Price = 100,
                    Quantity = 2,
                    Subtotal = 200
                }
            ]
        };
        cart.Items.First().CartId = cart.Id;
        _cartRepository.AddSeed(cart);

        var result = await _service.UpdateCartItemAsync(new UpdateCartItemRequestDto
        {
            UserId = userId,
            CartItemId = cart.Items.First().Id,
            NewQuantity = 3
        });

        Assert.Multiple(() =>
        {
            Assert.That(result.Items.Single().Quantity, Is.EqualTo(3));
            Assert.That(result.Items.Single().Subtotal, Is.EqualTo(300));
            Assert.That(result.TotalAmount, Is.EqualTo(300));
            Assert.That(_cartRepository.UpdateCallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task ClearCartAsync_WhenCartExists_RemovesItemsAndCoupon()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            AppliedCouponCode = "SAVE20",
            TotalAmount = 440,
            Items = [new CartItem { Id = Guid.NewGuid(), Quantity = 2, Price = 220, Subtotal = 440 }]
        };
        _cartRepository.AddSeed(cart);

        var result = await _service.ClearCartAsync(userId, restaurantId);

        Assert.Multiple(() =>
        {
            Assert.That(result.Items, Is.Empty);
            Assert.That(result.TotalAmount, Is.EqualTo(0));
            Assert.That(result.Currency, Is.EqualTo("INR"));
        });
    }

    [Test]
    public async Task CalculateTotalsAsync_ComputesTaxAndTotal()
    {
        var userId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RestaurantId = restaurantId,
            Items =
            [
                new CartItem { Id = Guid.NewGuid(), Subtotal = 200 },
                new CartItem { Id = Guid.NewGuid(), Subtotal = 300 }
            ]
        };
        _cartRepository.AddSeed(cart);

        var result = await _service.CalculateTotalsAsync(userId, restaurantId, taxPercentage: 10);

        Assert.Multiple(() =>
        {
            Assert.That(result.Subtotal, Is.EqualTo(500));
            Assert.That(result.Tax, Is.EqualTo(50));
            Assert.That(result.Total, Is.EqualTo(550));
            Assert.That(result.Currency, Is.EqualTo("INR"));
        });
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        private readonly Dictionary<Guid, Cart> _carts = new();
        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }

        public void AddSeed(Cart cart) => _carts[cart.Id] = cart;

        public Task<Cart?> GetCartByUserAndRestaurantAsync(Guid userId, Guid restaurantId, CancellationToken cancellationToken = default)
        {
            var cart = _carts.Values.FirstOrDefault(c => c.UserId == userId && c.RestaurantId == restaurantId);
            return Task.FromResult(cart);
        }

        public Task<Cart?> GetByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            _carts.TryGetValue(cartId, out var cart);
            return Task.FromResult(cart);
        }

        public Task AddAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            AddCallCount++;
            if (cart.Id == Guid.Empty)
                cart.Id = Guid.NewGuid();
            foreach (var item in cart.Items.Where(i => i.Id == Guid.Empty))
                item.Id = Guid.NewGuid();
            _carts[cart.Id] = cart;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            UpdateCallCount++;
            _carts[cart.Id] = cart;
            return Task.CompletedTask;
        }

        public Task<CartItem?> GetCartItemAsync(Guid cartItemId, CancellationToken cancellationToken = default)
        {
            var item = _carts.Values.SelectMany(c => c.Items).FirstOrDefault(i => i.Id == cartItemId);
            return Task.FromResult(item);
        }
    }

    private sealed class FakeMenuItemValidationService : IMenuItemValidationService
    {
        public MenuItemValidationResult Result { get; set; } = new() { IsValid = true, CurrentPrice = 100 };

        public Task<MenuItemValidationResult> ValidateMenuItemAsync(Guid restaurantId, Guid menuItemId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result);
        }
    }
}
