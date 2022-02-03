using Microsoft.AspNetCore.Identity;

namespace LoyalWalletv2.Domain.Models.AuthenticationModels;

public class ApplicationUser : IdentityUser
{
    public Company? Company { get; set; }
}