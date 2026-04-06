using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.ValueObjects;

namespace AdminService.Infrastructure.Persistence;

public static class AdminServiceDbContextSeed
{
    public static async Task SeedAsync(AdminServiceDbContext context)
    {
        if (!context.Users.Any())
        {
            var adminEmail = "admin@fooddelivery.com";
            var adminContact = ContactInfo.Create(adminEmail, "+1234567890");
            
            var adminUser = User.Create(
                adminEmail,
                "Admin@123",
                UserRole.Admin,
                adminContact
            );

            context.Users.Add(adminUser);
        }

        if (!context.Restaurants.Any())
        {
            var restaurant1Contact = ContactInfo.Create("pizza@palace.com", "+1234567891");
            var restaurant1Address = Address.Create(
                "123 Main St",
                "New York",
                "NY",
                "10001",
                "USA"
            );

            var restaurant1 = Restaurant.Create(
                "Pizza Palace",
                "Best pizza in town",
                restaurant1Address,
                restaurant1Contact
            );
            restaurant1.Approve("Approved after verification");

            var restaurant2Contact = ContactInfo.Create("burger@world.com", "+1234567892");
            var restaurant2Address = Address.Create(
                "456 Oak Ave",
                "Los Angeles",
                "CA",
                "90001",
                "USA"
            );

            var restaurant2 = Restaurant.Create(
                "Burger World",
                "Fresh burgers daily",
                restaurant2Address,
                restaurant2Contact
            );
            restaurant2.Approve("Approved after verification");

            context.Restaurants.AddRange(restaurant1, restaurant2);
        }

        await context.SaveChangesAsync();
    }
}
