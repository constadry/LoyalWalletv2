using Microsoft.AspNetCore.Identity;

namespace LoyalWalletv2.Domain.Models.AuthenticationModels;

public class User : IdentityUser
{
    public int EntityId { get; set; }
    public string? Token { get; set; }
}