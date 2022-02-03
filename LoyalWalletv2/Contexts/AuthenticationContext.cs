using LoyalWalletv2.Domain.Models.AuthenticationModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Contexts;

public class AuthenticationContext : IdentityDbContext<ApplicationUser>
{
    public AuthenticationContext(DbContextOptions<AuthenticationContext> options) : base(options)  
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)  
    {  
        base.OnModelCreating(builder);  
    }  
}