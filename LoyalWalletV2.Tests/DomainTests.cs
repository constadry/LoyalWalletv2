using LoyalWalletv2.Domain.Models;
using Xunit;

namespace LoyalWalletV2.Tests;

public class DomainTests
{
    private readonly Customer _customer;
    private readonly Employee _employee;

    public DomainTests()
    {
        var company = new Company
        {
            MaxCountOfStamps = 6,
            Id = 1
        };

        _customer = new Customer
        {
            PhoneNumber = "+7 951 8270 540",
            CompanyId = company.Id,
            Company = company
        };

        _employee = new Employee
        {
            CompanyId = company.Id,
        };
    }

    [Fact]
    public void AddOneStamp()
    {
        const uint expectedStamps = 1;

        _customer.AddStamp(_employee);

        Assert.Equal(expectedStamps, _customer.CountOfStamps);
        Assert.Equal(expectedStamps, _employee.CountOfStamps);
    }

    [Fact]
    public void AddSixStamps_AndTakeOnePresent()
    {
        const uint expectedPresents = 0;
        const uint expectedGivenPresents = 1;

        var i = 0;
        for (; i < 6; i++) _customer.AddStamp(_employee);
        
        _customer.TakePresent(_employee);

        Assert.Equal(expectedPresents, _customer.CountOfStoredPresents);
        Assert.Equal(expectedGivenPresents, _employee.CountOfPresents);
    }
}