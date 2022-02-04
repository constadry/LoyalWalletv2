using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Domain.Models;

public class Employee
{
    [Key] public int Id { get; set; }
    [Required] [MaxLength(100)] public string? Name { get; set; }
    [Required] [MaxLength(100)] public string? Surname { get; set; }
    [Required] public int CompanyId { get; set; }
    public bool Archived { get; set; }
}