using System;
using System.Collections.Generic;
using LoyalWalletv2;
using LoyalWalletv2.Contexts;
using Xunit;
using Xunit.Abstractions;

namespace LoyalWalletV2.Tests;

public class ApiTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ApiTests(ITestOutputHelper testOutputHelper, AppDbContext context)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async void AddNewCustomer()
    {
        // var values = new Dictionary<string, string>
        // {
        //     { "phoneNumber", "+7 (951) 8270 540" },
        //     { "companyId", "1" }
        // };
        //
        // var content = new FormUrlEncodedContent(values);
        //
        // using var requestMessage =
        //     new HttpRequestMessage(HttpMethod.Post, "https://localhost:7005/api/Osmi/clients/add");
        // requestMessage.Content = content;
        // var responseMessage = await _httpClient.SendAsync(requestMessage);
        // _testOutputHelper.WriteLine(requestMessage.Content.ToString());
    }
}