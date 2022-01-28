using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoyalWalletv2.Domain.Models;

public class Customer
{
    private uint _countOfStamps;
    private uint _countOfPresents;

    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(14)]
    public string PhoneNumber { get; set; }

    public int SerialNumber { get; set; }

    [Required] 
    public int CompanyId { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; }

    public bool Confirmed { get; set; }

    public uint CountOfStamps => _countOfStamps;

    public uint CountOfPresents => _countOfPresents;

    public void AddStamp()
    {
        if (CountOfStamps + 1 == Company.MaxCountOfStamps)
        {
            _countOfStamps = 0;
            _countOfPresents++;
        }
        else
        {
            _countOfStamps++;
        }
    }

    public void TakePresent()
    {
        _countOfPresents--;
    }
}