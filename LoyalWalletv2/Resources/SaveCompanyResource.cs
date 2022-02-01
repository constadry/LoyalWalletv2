using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Resources;

public class SaveCompanyResource
{
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}