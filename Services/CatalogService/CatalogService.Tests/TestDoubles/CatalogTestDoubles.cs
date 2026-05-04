using System.Reflection;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Domain.Interfaces;
using MassTransit;

namespace CatalogService.Tests.TestDoubles;

internal sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly Dictionary<Guid, Category> _categories = new();
    public bool DeleteResult { get; set; } = true;

    public void Add(Category category) => _categories[category.Id] = category;

    public Task<Category?> GetByIdAsync(Guid id) => Task.FromResult(_categories.GetValueOrDefault(id));

    public Task<List<Category>> GetByRestaurantAsync(Guid restaurantId) =>
        Task.FromResult(_categories.Values.Where(c => c.RestaurantId == restaurantId).ToList());

    public Task<Category?> GetByNameAsync(string name, Guid restaurantId) =>
        Task.FromResult(_categories.Values.FirstOrDefault(c => c.RestaurantId == restaurantId && c.Name == name));

    public Task<Category> CreateAsync(Category category)
    {
        if (category.Id == Guid.Empty)
            category.Id = Guid.NewGuid();
        _categories[category.Id] = category;
        return Task.FromResult(category);
    }

    public Task<Category> UpdateAsync(Category category)
    {
        _categories[category.Id] = category;
        return Task.FromResult(category);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        _categories.Remove(id);
        return Task.FromResult(DeleteResult);
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_categories.ContainsKey(id));
    public Task<bool> ExistsByNameAsync(string name, Guid restaurantId) =>
        Task.FromResult(_categories.Values.Any(c => c.RestaurantId == restaurantId && c.Name == name));
}

internal sealed class FakeMenuItemRepository : IMenuItemRepository
{
    private readonly Dictionary<Guid, MenuItem> _items = new();
    public bool DeleteResult { get; set; } = true;

    public void Add(MenuItem item) => _items[item.Id] = item;

    public Task<MenuItem?> GetByIdAsync(Guid id) => Task.FromResult(_items.GetValueOrDefault(id));

    public Task<List<MenuItem>> GetByRestaurantAsync(Guid restaurantId) =>
        Task.FromResult(_items.Values.Where(i => i.RestaurantId == restaurantId).ToList());

    public Task<List<MenuItem>> SearchAsync(string query) =>
        Task.FromResult(_items.Values.Where(i => i.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList());

    public Task<MenuItem> CreateAsync(MenuItem menuItem)
    {
        if (menuItem.Id == Guid.Empty)
            menuItem.Id = Guid.NewGuid();
        _items[menuItem.Id] = menuItem;
        return Task.FromResult(menuItem);
    }

    public Task<MenuItem> UpdateAsync(MenuItem menuItem)
    {
        _items[menuItem.Id] = menuItem;
        return Task.FromResult(menuItem);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        _items.Remove(id);
        return Task.FromResult(DeleteResult);
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_items.ContainsKey(id));
}

internal sealed class FakeRestaurantRepository : IRestaurantRepository
{
    private readonly Dictionary<Guid, Restaurant> _restaurants = new();

    public void Add(Restaurant restaurant) => _restaurants[restaurant.Id] = restaurant;

    public Task<List<Restaurant>> GetAllAsync() => Task.FromResult(_restaurants.Values.ToList());

    public Task<List<Restaurant>> GetFilteredAsync(RestaurantStatus? status, string? searchTerm, string? cuisine, decimal? minRating, string? city, bool? isVegetarianOnly)
    {
        IEnumerable<Restaurant> query = _restaurants.Values;
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(searchTerm)) query = query.Where(r => r.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(query.ToList());
    }

    public Task<Restaurant?> GetByIdAsync(Guid id) => Task.FromResult(_restaurants.GetValueOrDefault(id));
    public Task<Restaurant?> GetByNameAsync(string name) => Task.FromResult(_restaurants.Values.FirstOrDefault(r => r.Name == name));
    public Task<Restaurant?> GetByOwnerIdAsync(Guid ownerId) => Task.FromResult(_restaurants.Values.FirstOrDefault(r => r.OwnerId == ownerId));
    public Task<List<Restaurant>> GetListByOwnerIdAsync(Guid ownerId) => Task.FromResult(_restaurants.Values.Where(r => r.OwnerId == ownerId).ToList());
    public Task<List<Restaurant>> SearchByNameAsync(string query) => Task.FromResult(_restaurants.Values.Where(r => r.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList());
    public Task<List<Restaurant>> GetByCuisineAsync(CuisineType cuisine) => Task.FromResult(_restaurants.Values.Where(r => r.CuisineType == cuisine).ToList());
    public Task<List<Restaurant>> GetByStatusAsync(RestaurantStatus status) => Task.FromResult(_restaurants.Values.Where(r => r.Status == status).ToList());
    public Task<List<Restaurant>> GetByRatingAsync(decimal minRating) => Task.FromResult(_restaurants.Values.Where(r => r.Rating >= minRating).ToList());

    public Task<Restaurant> CreateAsync(Restaurant restaurant)
    {
        if (restaurant.Id == Guid.Empty)
            restaurant.Id = Guid.NewGuid();
        _restaurants[restaurant.Id] = restaurant;
        return Task.FromResult(restaurant);
    }

    public Task<Restaurant> UpdateAsync(Restaurant restaurant)
    {
        _restaurants[restaurant.Id] = restaurant;
        return Task.FromResult(restaurant);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        _restaurants.Remove(id);
        return Task.FromResult(true);
    }

    public Task<bool> ExistsAsync(Guid id) => Task.FromResult(_restaurants.ContainsKey(id));
}

internal class RecordingPublishEndpoint : DispatchProxy
{
    public IPublishEndpoint Endpoint { get; private set; } = null!;
    public List<object> PublishedMessages { get; } = new();

    public static RecordingPublishEndpoint Create()
    {
        var endpoint = DispatchProxy.Create<IPublishEndpoint, RecordingPublishEndpoint>();
        var proxy = (RecordingPublishEndpoint)(object)endpoint;
        proxy.Endpoint = endpoint;
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        if (targetMethod?.Name == nameof(IPublishEndpoint.Publish) && args?.Length > 0 && args[0] is object message)
            PublishedMessages.Add(message);

        if (targetMethod?.ReturnType == typeof(Task))
            return Task.CompletedTask;

        return targetMethod?.ReturnType.IsValueType == true
            ? Activator.CreateInstance(targetMethod.ReturnType)
            : null;
    }
}
