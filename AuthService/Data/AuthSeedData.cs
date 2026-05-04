using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public static class AuthSeedData
{
    public static void Seed(AppDbContext context, IPasswordHasher<User> passwordHasher)
    {
        context.Database.Migrate();

        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { Name = "Admin" },
                new Role { Name = "Cashier" },
                new Role { Name = "User" });
        }

        if (!context.Stores.Any())
        {
            context.Stores.AddRange(
                new Store { Name = "RetailPOS Urban Market", Location = "MG Road, Bengaluru" },
                new Store { Name = "RetailPOS Express", Location = "Indiranagar, Bengaluru" });
        }

        context.SaveChanges();

        // If our primary admin exists, we can stop
        if (context.Users.Any(u => u.Email == "admin@pos.com"))
            return;

        var users = new[]
        {
            new User
            {
                Name = "System Admin",
                Email = "admin@pos.com",
                RoleId = context.Roles.First(r => r.Name == "Admin").Id,
                StoreId = context.Stores.First().Id
            },
            new User
            {
                Name = "Store Admin",
                Email = "admin@retailpos.local",
                RoleId = context.Roles.First(r => r.Name == "Admin").Id,
                StoreId = context.Stores.First().Id
            },
            new User
            {
                Name = "Counter Cashier",
                Email = "cashier@retailpos.local",
                RoleId = context.Roles.First(r => r.Name == "Cashier").Id,
                StoreId = context.Stores.First().Id
            },
            new User
            {
                Name = "Priya Nair",
                Email = "priya@retailpos.local",
                RoleId = context.Roles.First(r => r.Name == "User").Id,
                StoreId = context.Stores.First().Id
            }
        };

        foreach (var user in users)
        {
            user.PasswordHash = passwordHasher.HashPassword(user, "Retail@123");
        }

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
