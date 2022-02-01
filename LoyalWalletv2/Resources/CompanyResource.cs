using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Resources;

public class CompanyResource
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(14)]
    public string? PhoneNumber { get; set; }
}