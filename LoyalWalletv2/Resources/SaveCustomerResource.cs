using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Resources;

public class SaveCustomerResource
{
    [Required]
    [MaxLength(14)] 
    public string PhoneNumber { get; set; }

    [MaxLength(100)]
    public string? CompanyId { get; set; }
}