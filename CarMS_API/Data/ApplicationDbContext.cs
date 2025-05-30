using CarMS_API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarMS_API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<CarHistory> CarHistories { get; set; }
        public DbSet<CarMaintenance> CarMaintenances { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Approval> Approvals { get; set; }
        public DbSet<TestDrive> TestDrives { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
    }
}
