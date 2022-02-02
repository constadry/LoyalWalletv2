using LoyalWalletv2.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
        // Database.EnsureDeleted();
        Database.EnsureCreated();
    }
    
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Code> Codes { get; set; }
}