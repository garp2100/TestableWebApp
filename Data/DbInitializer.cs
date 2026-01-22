using Microsoft.AspNetCore.Identity;
using TestableWebApp.Models;

namespace TestableWebApp.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();
        
        // Create roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
        
        // Create admin user
        var adminEmail = "admin@test.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        
        // Create regular user
        var userEmail = "user@test.com";
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = userEmail,
                Email = userEmail,
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }
        
        // Seed products if none exist
        if (!context.Products.Any())
        {
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Laptop Pro 15",
                    Description = "High-performance laptop with 15-inch display",
                    Price = 1299.99m,
                    Category = "Electronics",
                    StockQuantity = 50,
                    IsActive = true,
                    ImageUrl = "/images/laptop.jpg"
                },
                new Product
                {
                    Name = "Wireless Mouse",
                    Description = "Ergonomic wireless mouse with long battery life",
                    Price = 29.99m,
                    Category = "Electronics",
                    StockQuantity = 200,
                    IsActive = true,
                    ImageUrl = "/images/mouse.jpg"
                },
                new Product
                {
                    Name = "Programming T-Shirt",
                    Description = "Cotton t-shirt with programming humor",
                    Price = 24.99m,
                    Category = "Clothing",
                    StockQuantity = 100,
                    IsActive = true,
                    ImageUrl = "/images/tshirt.jpg"
                },
                new Product
                {
                    Name = "Clean Code Book",
                    Description = "A handbook of agile software craftsmanship",
                    Price = 39.99m,
                    Category = "Books",
                    StockQuantity = 75,
                    IsActive = true,
                    ImageUrl = "/images/book.jpg"
                },
                new Product
                {
                    Name = "Standing Desk",
                    Description = "Adjustable height standing desk",
                    Price = 499.99m,
                    Category = "Home & Garden",
                    StockQuantity = 25,
                    IsActive = true,
                    ImageUrl = "/images/desk.jpg"
                },
                new Product
                {
                    Name = "Yoga Mat",
                    Description = "Non-slip exercise yoga mat",
                    Price = 34.99m,
                    Category = "Sports",
                    StockQuantity = 150,
                    IsActive = true,
                    ImageUrl = "/images/yogamat.jpg"
                },
                new Product
                {
                    Name = "Building Blocks Set",
                    Description = "500-piece creative building blocks",
                    Price = 49.99m,
                    Category = "Toys",
                    StockQuantity = 80,
                    IsActive = true,
                    ImageUrl = "/images/blocks.jpg"
                },
                new Product
                {
                    Name = "Organic Coffee Beans",
                    Description = "Premium organic coffee beans, 1lb bag",
                    Price = 18.99m,
                    Category = "Food & Beverages",
                    StockQuantity = 200,
                    IsActive = true,
                    ImageUrl = "/images/coffee.jpg"
                },
                new Product
                {
                    Name = "Vitamin D Supplements",
                    Description = "Daily vitamin D3 supplements, 90 count",
                    Price = 14.99m,
                    Category = "Health & Beauty",
                    StockQuantity = 300,
                    IsActive = true,
                    ImageUrl = "/images/vitamins.jpg"
                },
                new Product
                {
                    Name = "Discontinued Product",
                    Description = "This product is no longer available",
                    Price = 9.99m,
                    Category = "Electronics",
                    StockQuantity = 0,
                    IsActive = false,
                    ImageUrl = "/images/discontinued.jpg"
                }
            };
            
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}
