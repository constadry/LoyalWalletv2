using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Domain.Models;

public class Company
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(14)]
    public string? PhoneNumber { get; set; }

    [MaxLength(100)] 
    public string? Address { get; set; }

    public IList<Customer> Customers { get; set; } = new List<Customer>();

    public uint MaxCountOfStamps { get; set; } = 6;
}