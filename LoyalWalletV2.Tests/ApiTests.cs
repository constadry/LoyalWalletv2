using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletV2.Tests.Extensions;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LoyalWalletV2.Tests;

public class ApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ApiTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async void AddLocation_AddEmployee()
    {
        await using var application = new CustomWebApplicationFactory<Program>();
        using var client = application.CreateClient();

        var loginRequest = new LoginModel
        {
            Email = "kostya.adrianov@gmail.com",
            Password = "Password123#"
        };

        var serializedValues = JsonConvert.SerializeObject(loginRequest);
        
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, "api/Authenticate/login");

        requestMessage.Content = new StringContent(
            serializedValues,
            Encoding.UTF8,
            "application/json");
        
        var response = await client.SendAsync(requestMessage);
        
        var responseSerialised = await response.Content.ReadAsStringAsync();

        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(responseSerialised);
        _testOutputHelper.WriteLine(responseSerialised);
    
        using var requestMessage2 =
            new HttpRequestMessage(HttpMethod.Get, "/api/Customer/1/all-stamps-count");
        
        requestMessage2.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        
        response = await client.SendAsync(requestMessage2);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("1", await response.Content.ReadAsStringAsync());
    }
}

public class LoginResponse
{
    public string? Token { get; set; }
    public string? Expiration { get; set; }
    public int CompanyId { get; set; }
}