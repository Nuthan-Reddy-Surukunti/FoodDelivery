using AutoMapper;
using CatalogService.Application.DTOs.Category;
using CatalogService.Application.DTOs.MenuItem;
using CatalogService.Application.DTOs.Restaurant;
using CatalogService.Application.Exceptions;
using CatalogService.Application.Mappers;
using CatalogService.Application.Services;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using QuickBite.Shared.Events.Catalog;

namespace CatalogService.Tests;

[TestFixture]
public class CatalogApplicationServiceTests
{
    private IMapper _mapper = null!;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new CategoryMappingProfile());
            cfg.AddProfile(new MenuItemMappingProfile());
            cfg.AddProfile(new RestaurantMappingProfile());
        }, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Test]
    public void CategoryService_CreateCategoryAsync_RestaurantPartnerCannotCreateForAnotherOwner()
    {
        var categoryRepo = new FakeCategoryRepository();
        var restaurantRepo = new FakeRestaurantRepository();
        var service = new CategoryService(categoryRepo, restaurantRepo, _mapper);
        var ownerId = Guid.NewGuid();
        restaurantRepo.Add(CreateRestaurant(ownerId: ownerId));

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.CreateCategoryAsync(
                new CreateCategoryDto { Name = "Snacks", RestaurantId = restaurantRepo.GetAllAsync().Result.Single().Id },
                Guid.NewGuid(),
                "RestaurantPartner"));
    }

    [Test]
    public void CategoryService_UpdateCategoryAsync_ThrowsForDuplicateCategoryName()
    {
        var categoryRepo = new FakeCategoryRepository();
        var restaurantRepo = new FakeRestaurantRepository();
        var restaurant = CreateRestaurant();
        restaurantRepo.Add(restaurant);

        var existing = new Category { Id = Guid.NewGuid(), RestaurantId = restaurant.Id, Name = "Starters" };
        var target = new Category { Id = Guid.NewGuid(), RestaurantId = restaurant.Id, Name = "Main Course" };
        categoryRepo.Add(existing);
        categoryRepo.Add(target);

        var service = new CategoryService(categoryRepo, restaurantRepo, _mapper);

        Assert.ThrowsAsync<DuplicateCategoryException>(async () =>
            await service.UpdateCategoryAsync(
                new UpdateCategoryDto { Id = target.Id, Name = "Starters" },
                Guid.NewGuid(),
                "Admin"));
    }

    [Test]
    public void CategoryService_DeleteCategoryAsync_ThrowsWhenCategoryHasMenuItems()
    {
        var categoryRepo = new FakeCategoryRepository();
        var restaurantRepo = new FakeRestaurantRepository();
        var restaurant = CreateRestaurant();
        restaurantRepo.Add(restaurant);
        var category = new Category
        {
            Id = Guid.NewGuid(),
            RestaurantId = restaurant.Id,
            Name = "Beverages",
            MenuItems = [new MenuItem { Id = Guid.NewGuid(), Name = "Lime Soda", RestaurantId = restaurant.Id }]
        };
        categoryRepo.Add(category);
        var service = new CategoryService(categoryRepo, restaurantRepo, _mapper);

        Assert.ThrowsAsync<InvalidRestaurantDataException>(async () =>
            await service.DeleteCategoryAsync(category.Id, Guid.NewGuid(), "Admin"));
    }

    [Test]
    public async Task MenuItemService_CreateMenuItemAsync_PublishesCreatedEvent()
    {
        var menuRepo = new FakeMenuItemRepository();
        var restaurantRepo = new FakeRestaurantRepository();
        var publisher = RecordingPublishEndpoint.Create();
        var restaurant = CreateRestaurant();
        restaurantRepo.Add(restaurant);
        var service = new MenuItemService(menuRepo, restaurantRepo, _mapper, publisher.Endpoint);

        var result = await service.CreateMenuItemAsync(new CreateMenuItemDto
        {
            Name = "Paneer Tikka",
            Description = "Spicy grilled paneer",
            RestaurantId = restaurant.Id,
            CategoryId = Guid.NewGuid(),
            Price = 199,
            IsVeg = true
        }, Guid.NewGuid(), "Admin");

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Paneer Tikka"));
            Assert.That(publisher.PublishedMessages.OfType<MenuItemCreatedEvent>().Count(), Is.EqualTo(1));
            Assert.That(publisher.PublishedMessages.OfType<MenuItemCreatedEvent>().Single().RestaurantId, Is.EqualTo(restaurant.Id));
        });
    }

    [Test]
    public async Task MenuItemService_GetMenuItemsByRestaurantAsync_FiltersUnavailableForCustomers()
    {
        var menuRepo = new FakeMenuItemRepository();
        var restaurantRepo = new FakeRestaurantRepository();
        var publisher = RecordingPublishEndpoint.Create();
        var restaurant = CreateRestaurant(status: RestaurantStatus.Active);
        restaurantRepo.Add(restaurant);
        menuRepo.Add(new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Available Item",
            RestaurantId = restaurant.Id,
            CategoryId = Guid.NewGuid(),
            Price = 100,
            AvailabilityStatus = ItemAvailabilityStatus.Available
        });
        menuRepo.Add(new MenuItem
        {
            Id = Guid.NewGuid(),
            Name = "Hidden Item",
            RestaurantId = restaurant.Id,
            CategoryId = Guid.NewGuid(),
            Price = 120,
            AvailabilityStatus = ItemAvailabilityStatus.OutOfStock
        });

        var service = new MenuItemService(menuRepo, restaurantRepo, _mapper, publisher.Endpoint);
        var result = await service.GetMenuItemsByRestaurantAsync(restaurant.Id, "Customer", Guid.NewGuid());

        Assert.That(result.Select(x => x.Name), Is.EquivalentTo(new[] { "Available Item" }));
    }

    [Test]
    public async Task RestaurantService_CreateRestaurantAsync_RestaurantPartnerSetsOwnerAndPending()
    {
        var restaurantRepo = new FakeRestaurantRepository();
        var publisher = RecordingPublishEndpoint.Create();
        var service = new RestaurantService(restaurantRepo, _mapper, publisher.Endpoint);
        var userId = Guid.NewGuid();

        var result = await service.CreateRestaurantAsync(new CreateRestaurantDto
        {
            Name = "Spice Hub",
            Address = "Addr",
            City = "Hyderabad",
            ServiceZoneId = "HZ-1",
            CuisineType = CuisineType.Indian
        }, userId, "RestaurantPartner");

        Assert.Multiple(() =>
        {
            Assert.That(result.Status, Is.EqualTo(RestaurantStatus.Pending));
            Assert.That(publisher.PublishedMessages.OfType<RestaurantCreatedEvent>().Count(), Is.EqualTo(1));
            Assert.That(publisher.PublishedMessages.OfType<RestaurantCreatedEvent>().Single().OwnerId, Is.EqualTo(userId));
        });
    }

    [Test]
    public async Task RestaurantService_UpdateRestaurantAsync_AdminCanChangeStatusAndPublishesEvent()
    {
        var restaurantRepo = new FakeRestaurantRepository();
        var publisher = RecordingPublishEndpoint.Create();
        var restaurant = CreateRestaurant(status: RestaurantStatus.Pending);
        restaurantRepo.Add(restaurant);
        var service = new RestaurantService(restaurantRepo, _mapper, publisher.Endpoint);

        var result = await service.UpdateRestaurantAsync(restaurant.Id, new UpdateRestaurantDto
        {
            Name = "Updated Name",
            Status = RestaurantStatus.Active
        }, Guid.NewGuid(), "Admin");

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("Updated Name"));
            Assert.That(result.Status, Is.EqualTo(RestaurantStatus.Active));
            Assert.That(publisher.PublishedMessages.OfType<RestaurantUpdatedEvent>().Count(), Is.EqualTo(1));
        });
    }

    [Test]
    public void RestaurantService_UpdateRestaurantAsync_RestaurantPartnerCannotChangeStatus()
    {
        var restaurantRepo = new FakeRestaurantRepository();
        var publisher = RecordingPublishEndpoint.Create();
        var ownerId = Guid.NewGuid();
        var restaurant = CreateRestaurant(ownerId: ownerId, status: RestaurantStatus.Active);
        restaurantRepo.Add(restaurant);
        var service = new RestaurantService(restaurantRepo, _mapper, publisher.Endpoint);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await service.UpdateRestaurantAsync(restaurant.Id, new UpdateRestaurantDto
            {
                Status = RestaurantStatus.Suspended
            }, ownerId, "RestaurantPartner"));
    }

    private static Restaurant CreateRestaurant(Guid? ownerId = null, RestaurantStatus status = RestaurantStatus.Active)
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Test Restaurant",
            Address = "Addr",
            City = "City",
            CuisineType = CuisineType.Indian,
            Status = status,
            OwnerId = ownerId
        };
    }
}
