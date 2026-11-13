namespace Domain
{
    using Domain.Entities;
    using Domain.Entities.Supporting;
    using global::Entities.Supporting.Seeding;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Washer> Washers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Wash> Washes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customer -> User
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Washer -> User
            modelBuilder.Entity<Washer>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle -> Customer
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => new { v.CustomerId, v.Registration })
                .IsUnique();

            // Booking -> Customer, Vehicle, Service, Washer
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Washer)
                .WithMany(w => w.Bookings)
                .HasForeignKey(b => b.WasherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wash -> Booking (one-to-one, optional), Customer, Vehicle, Service, Washer
            modelBuilder.Entity<Wash>()
                .HasOne(w => w.Booking)
                .WithOne(b => b.Wash)
                .HasForeignKey<Wash>(w => w.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wash>()
                .HasOne(w => w.Customer)
                .WithMany(c => c.Washes)
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wash>()
                .HasOne(w => w.Vehicle)
                .WithMany(v => v.Washes)
                .HasForeignKey(w => w.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wash>()
                .HasOne(w => w.Service)
                .WithMany(s => s.Washes)
                .HasForeignKey(w => w.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wash>()
                .HasOne(w => w.Washer)
                .WithMany(w => w.Washes)
                .HasForeignKey(w => w.WasherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}