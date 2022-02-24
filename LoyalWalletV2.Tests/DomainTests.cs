using System.Net.Http;
using System.Threading.Tasks;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Services;
using Xunit;
using Xunit.Abstractions;

namespace LoyalWalletV2.Tests;

public class DomainTests
{
    private readonly Customer _customer;
    private readonly Employee _employee;
    private readonly ITestOutputHelper _testOutputHelper;
    
    public DomainTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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

        _customer.DoStamp(_employee);

        Assert.Equal(expectedStamps, _customer.CountOfStamps);
        Assert.Equal(expectedStamps, _employee.CountOfStamps);
    }

    [Fact]
    public void AddSixStamps_AndTakeOnePresent()
    {
        const uint expectedPresents = 0;
        const uint expectedGivenPresents = 1;

        var i = 0;
        for (; i < 6; i++) _customer.DoStamp(_employee);
        
        _customer.TakePresent(_employee);

        Assert.Equal(expectedPresents, _customer.CountOfStoredPresents);
        Assert.Equal(expectedGivenPresents, _employee.CountOfPresents);
    }

    [Fact]
    public async void TokenOsmi()
    {
        var httpClient = new HttpClient();
        var tokenService = new TokenService(httpClient);

        var token = await tokenService.GetTokenAsync();
        _testOutputHelper.WriteLine(token);
    }
}