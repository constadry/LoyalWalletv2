using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyalWalletv2.Domain.Models;

public class Customer
{
    private uint _countOfStamps;
    private uint _countOfPurchases;
    private uint _countOfStoredPresents;
    private uint _countOfGivenPresents;

    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public int SerialNumber { get; set; }

    [Required] 
    public int CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }

    public bool Confirmed { get; set; }

    public uint CountOfStamps => _countOfStamps;
    public uint CountOfPurchases => _countOfPurchases;
    public uint CountOfStoredPresents => _countOfStoredPresents;
    public uint CountOfGivenPresents => _countOfGivenPresents;
    public DateTime FirstTimePurchase { get; set; }
    public DateTime LastTimePurchase { get; set; }

    public void AddStamp(Employee employee)
    {
        if (Company != null && CountOfStamps + 1 == Company.MaxCountOfStamps)
        {
            _countOfStamps = 0;
            _countOfStoredPresents++;
        }
        else
        {
            _countOfStamps++;
        }

        if (_countOfPurchases == 0)
            FirstTimePurchase = DateTime.Now;

        LastTimePurchase = DateTime.Now;
        _countOfPurchases++;

        employee.CountOfStamps += 1;
    }

    public void TakePresent(Employee employee)
    {
        _countOfStoredPresents--;
        _countOfGivenPresents++;
        employee.CountOfPresents += 1;
    }
}