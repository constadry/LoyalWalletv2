using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
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
    public async void AllStampCount()
    {
        // var application = new CustomWebApplicationFactory<Program>();
        // var client = application.CreateClient();
        //
        // var loginResponse = await Login(client);
        //
        // using var requestMessage =
        //     new HttpRequestMessage(HttpMethod.Get, "/api/Customer/1/all-stamps-count");
        //
        // requestMessage.Headers.Authorization =
        //     new AuthenticationHeaderValue("Bearer", loginResponse.Token);
        //
        // var response = await client.SendAsync(requestMessage);
        //
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // Assert.Equal("1", await response.Content.ReadAsStringAsync());
        
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
    
    // [Fact]
    // public async void AllPresentsCount()
    // {
    //     await using var application = new CustomWebApplicationFactory<Program>();
    //     using var client = application.CreateClient();
    //
    //     var loginResponse = await Login(client);
    //
    //     using var requestMessage =
    //         new HttpRequestMessage(HttpMethod.Get, "/api/Customer/1/all-presents-count");
    //     
    //     requestMessage.Headers.Authorization =
    //         new AuthenticationHeaderValue("Bearer", loginResponse.Token);
    //     HttpResponseMessage response;
    //     try
    //     {
    //         response = await client.SendAsync(requestMessage);
    //     }
    //     catch (Exception e)
    //     {
    //         _testOutputHelper.WriteLine(e.Message);
    //         throw;
    //     }
    //
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //     Assert.Equal("1", await response.Content.ReadAsStringAsync());
    //     // _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());
    // }
    //
    // private async Task<LoginResponse> Login(HttpClient client)
    // {
    //     var loginRequest = new LoginModel
    //     {
    //         Email = "kostya.adrianov@gmail.com",
    //         Password = "Password123#"
    //     };
    //
    //     var serializedValues = JsonConvert.SerializeObject(loginRequest);
    //     
    //     using var requestMessage =
    //         new HttpRequestMessage(HttpMethod.Post, "api/Authenticate/login");
    //
    //     requestMessage.Content = new StringContent(
    //         serializedValues,
    //         Encoding.UTF8,
    //         "application/json");
    //     
    //     var response = await client.SendAsync(requestMessage);
    //     
    //     var responseSerialised = await response.Content.ReadAsStringAsync();
    //
    //     _testOutputHelper.WriteLine(responseSerialised);
    //     return JsonConvert.DeserializeObject<LoginResponse>(responseSerialised);
    // }
}

public class LoginResponse
{
    public string? Token { get; set; }
    public string? Expiration { get; set; }
    public int CompanyId { get; set; }
}