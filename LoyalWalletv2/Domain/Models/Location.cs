using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Domain.Models;

public class Location
{    
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string? Name { get; set; }

    [Required]
    public int CompanyId { get; set; }
}