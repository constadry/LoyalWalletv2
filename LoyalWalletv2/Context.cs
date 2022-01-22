using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2;

public sealed class Context : DbContext
{
    public Context(DbContextOptions<Context> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}