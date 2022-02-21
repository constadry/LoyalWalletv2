using LoyalWalletv2.Domain.Models;
using Xunit;

namespace LoyalWalletV2.Tests;

public class DomainTests
{
    private Company _company;
    private Customer _customer;

    public DomainTests()
    {
        _company = new Company
        {
            MaxCountOfStamps = 6,
            Id = 1
        };

        _customer = new Customer
        {
            PhoneNumber = "+7 951 8270 540",
            CompanyId = 1,
            Company = _company
        };
    }

    [Fact]
    public void AddOneStamp()
    {
        const uint expectedStamps = 1;

        _customer.AddStamp();

        Assert.Equal(expectedStamps, _customer.CountOfStamps);
    }

    [Fact]
    public void AddSixStamps_AndTakeOnePresent()
    {
        const uint expectedPresents = 0;

        var i = 0;
        for (; i < 6; i++) _customer.AddStamp();
        
        _customer.TakePresent();

        Assert.Equal(expectedPresents, _customer.CountOfStoredPresents);
    }
}