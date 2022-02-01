using System.ComponentModel.DataAnnotations;

namespace LoyalWalletv2.Resources;

public class CustomerResource
{
    public int Id { get; set; }
    public string? PhoneNumber { get; set; }
    public int CompanyId { get; set; }
    public bool Confirmed { get; set; }
    public uint CountOfStamps { get; set; }
    public uint CountOfPresents { get; set; }
}