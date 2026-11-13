namespace Entities.Supporting.Seeding
{
    using Domain;
    using Domain.Entities;
    using Domain.Entities.Enums;
    using Domain.Entities.Supporting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Seeds roles, test users, and test data for every entity.
    /// </summary>
    public static class IdentitySeeder
    {
        private const string DefaultPassword = "#Systems97";

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            // Roles
            string[] roles = { StaticUserRoles.ADMIN, StaticUserRoles.CUSTOMER, StaticUserRoles.WASHER };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
            }

            // Users
            await CreateUserAsync(userManager, "admin@tebzacarwash.local", "Admin", "Tebza", "0810000001", StaticUserRoles.ADMIN);

            var washerUsers = new[]
            {
                await CreateUserAsync(userManager, "washer1@tebzacarwash.local", "Sipho", "Dlamini", "0810000002", StaticUserRoles.WASHER),
                await CreateUserAsync(userManager, "washer2@tebzacarwash.local", "Thabo", "Mokoena", "0810000003", StaticUserRoles.WASHER),
                await CreateUserAsync(userManager, "washer3@tebzacarwash.local", "Lerato", "Nkosi", "0810000004", StaticUserRoles.WASHER)
            };

            var customerUsers = new[]
            {
                await CreateUserAsync(userManager, "customer1@tebzacarwash.local", "John", "Smith", "0820000001", StaticUserRoles.CUSTOMER),
                await CreateUserAsync(userManager, "customer2@tebzacarwash.local", "Naledi", "Khumalo", "0820000002", StaticUserRoles.CUSTOMER),
                await CreateUserAsync(userManager, "customer3@tebzacarwash.local", "Pieter", "van Wyk", "0820000003", StaticUserRoles.CUSTOMER)
            };

            // Washers
            foreach (var u in washerUsers)
            {
                if (!await db.Set<Washer>().AnyAsync(w => w.UserId == u.Id))
                    db.Set<Washer>().Add(new Washer { UserId = u.Id });
            }

            // Customers
            foreach (var u in customerUsers)
            {
                if (!await db.Set<Customer>().AnyAsync(c => c.UserId == u.Id))
                    db.Set<Customer>().Add(new Customer { UserId = u.Id });
            }

            await db.SaveChangesAsync();

            // Services
            if (!await db.Set<Service>().AnyAsync())
            {
                db.Set<Service>().AddRange(
                    new Service { Name = "Basic Wash", Description = "Exterior wash and dry", Price = 80m, DurationMinutes = 20 },
                    new Service { Name = "Full Wash", Description = "Exterior and interior clean", Price = 150m, DurationMinutes = 45 },
                    new Service { Name = "Interior Clean", Description = "Vacuum and interior wipe-down", Price = 100m, DurationMinutes = 30 },
                    new Service { Name = "Polish & Wax", Description = "Wash, polish and wax finish", Price = 300m, DurationMinutes = 90 });
                await db.SaveChangesAsync();
            }

            var customers = await db.Set<Customer>().OrderBy(c => c.Id).ToListAsync();
            var washers = await db.Set<Washer>().OrderBy(w => w.Id).ToListAsync();
            var serviceList = await db.Set<Service>().OrderBy(s => s.Id).ToListAsync();

            // Vehicles
            if (!await db.Set<Vehicle>().AnyAsync() && customers.Count >= 3)
            {
                db.Set<Vehicle>().AddRange(
                    new Vehicle { CustomerId = customers[0].Id, Registration = "GP 123 ABC", Make = "Toyota", Model = "Corolla", Colour = "White", Type = VehicleType.Sedan },
                    new Vehicle { CustomerId = customers[0].Id, Registration = "GP 456 DEF", Make = "VW", Model = "Polo", Colour = "Silver", Type = VehicleType.Sedan },
                    new Vehicle { CustomerId = customers[1].Id, Registration = "GP 789 GHI", Make = "Ford", Model = "Ranger", Colour = "Black", Type = VehicleType.Sedan },
                    new Vehicle { CustomerId = customers[2].Id, Registration = "GP 321 JKL", Make = "BMW", Model = "320i", Colour = "Blue", Type = VehicleType.Sedan });
                await db.SaveChangesAsync();
            }

            var vehicles = await db.Set<Vehicle>().OrderBy(v => v.Id).ToListAsync();

            // Bookings
            if (!await db.Set<Booking>().AnyAsync() && vehicles.Count >= 4 && washers.Count >= 2 && serviceList.Count >= 4)
            {
                db.Set<Booking>().AddRange(
                    new Booking { CustomerId = vehicles[0].CustomerId, VehicleId = vehicles[0].Id, ServiceId = serviceList[0].Id, WasherId = washers[0].Id, ScheduledAt = DateTime.UtcNow.AddDays(1).AddHours(9) },
                    new Booking { CustomerId = vehicles[2].CustomerId, VehicleId = vehicles[2].Id, ServiceId = serviceList[1].Id, WasherId = washers[1].Id, ScheduledAt = DateTime.UtcNow.AddDays(1).AddHours(11) },
                    new Booking { CustomerId = vehicles[3].CustomerId, VehicleId = vehicles[3].Id, ServiceId = serviceList[3].Id, ScheduledAt = DateTime.UtcNow.AddDays(2).AddHours(10) });
                await db.SaveChangesAsync();
            }

            var firstBooking = await db.Set<Booking>().OrderBy(b => b.Id).FirstOrDefaultAsync();

            // Washes
            if (!await db.Set<Wash>().AnyAsync() && firstBooking is not null && vehicles.Count >= 4 && washers.Count >= 3)
            {
                db.Set<Wash>().AddRange(
                    new Wash
                    {
                        BookingId = firstBooking.Id,
                        CustomerId = vehicles[0].CustomerId,
                        VehicleId = vehicles[0].Id,
                        ServiceId = serviceList[0].Id,
                        WasherId = washers[0].Id,
                        Price = serviceList[0].Price
                    },
                    new Wash
                    {
                        CustomerId = vehicles[1].CustomerId,
                        VehicleId = vehicles[1].Id,
                        ServiceId = serviceList[1].Id,
                        WasherId = washers[1].Id,
                        Price = serviceList[1].Price
                    },
                    new Wash
                    {
                        CustomerId = vehicles[3].CustomerId,
                        VehicleId = vehicles[3].Id,
                        ServiceId = serviceList[2].Id,
                        WasherId = washers[2].Id,
                        Price = serviceList[2].Price
                    });
                await db.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> CreateUserAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string firstName,
            string lastName,
            string phone,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing is not null)
            {
                // Resets password to DefaultPassword (remove this block after first run)
                var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                await userManager.ResetPasswordAsync(existing, token, DefaultPassword);
                return existing;
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phone
            };

            var result = await userManager.CreateAsync(user, DefaultPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);

            return user;
        }
    }
}