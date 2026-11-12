namespace Domain
{
  using Domain.Entities;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
  using Microsoft.EntityFrameworkCore;

  public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
  {
    public DbSet<Log> Logs { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Runner> Runners { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // Seed Initial Data
      //Seeding.Seed(modelBuilder);

      base.OnModelCreating(modelBuilder);


      // Identity Configuration
      modelBuilder.Entity<IdentityUserToken<string>>()
          .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

      modelBuilder.Entity<IdentityUserLogin<string>>()
          .HasKey(l => new { l.LoginProvider, l.ProviderKey });

      modelBuilder.Entity<IdentityUserRole<string>>()
          .HasKey(r => new { r.UserId, r.RoleId });

      modelBuilder.Entity<IdentityUserClaim<string>>()
          .HasKey(c => c.Id);

      modelBuilder.Entity<IdentityRoleClaim<string>>()
          .HasKey(rc => rc.Id);

      // Customer-User Relationship
      modelBuilder.Entity<Customer>()
          .HasOne(c => c.User)
          .WithMany()
          .HasForeignKey(c => c.UserId)
          .OnDelete(DeleteBehavior.Restrict);

    }
  }
}