using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

[Authorize(Roles = nameof(EUserRoles.Admin))]
public class OsmiApiController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private const int serialNumFormat = 10000000;

    public OsmiApiController(HttpClient httpClient, AppDbContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }

    [HttpPost]
    [Route("cards/generate")]
    public async Task OsmiCardGenerate([FromBody] string phoneNumber, int companyId)
    {
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

        if (existingCustomer is not null)
        {
            if (existingCustomer.Confirmed)
            {
                var labels = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "label", "Id клиета" },
                    { "value", $"{existingCustomer.Id}" },
                    { "label", "Количество штампов" },
                    { "value", $"{existingCustomer.CountOfStamps} / {existingCustomer.Company.MaxCountOfStamps}" },
                    { "label", "Номер телефона" },
                    { "value", $"{existingCustomer.PhoneNumber}" },
                    { "label", "Id ресторана" },
                    { "value", $"{existingCustomer.CompanyId}" }
                });

                var barcode = JsonSerializer.Serialize(new KeyValuePair<string, string>("message", $"{existingCustomer.SerialNumber}"));

                var values = new Dictionary<string, string>
                {
                    { "noSharing", "false" },
                    { "values", labels},
                    { "barcode",  barcode}
                };
                var content = new FormUrlEncodedContent(values);
                
                //6tampCardMain?
                using (var requestMessage =
                       new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                               + "passes/${card_id}/6tampCardMain?withValues=true"))
                {
                    requestMessage.Content = content;
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
    
                    await _httpClient.SendAsync(requestMessage);
                }

                await OsmiSendCardOnSms(
                    existingCustomer.SerialNumber,
                    existingCustomer.PhoneNumber);
            }
        }
    }

    [HttpPost]
    [Route("register-client")]
    public async Task CustomerRegister([FromBody] string phoneNumber, int companyId)
    {
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);

        if (existingCustomer is not null)
        {
            await GenerateCustomer(phoneNumber, companyId);

            var values = new Dictionary<string, string>
            {
                { "smsText", "Ваш пинкод для транзакции {pin}" },
                { "length", "4" }
            };

            var content = new FormUrlEncodedContent(values);

            using (var requestMessage =
                   new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                           + $"/activation/sendpin/{phoneNumber}"))
            {
                requestMessage.Content = content;
                requestMessage.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);

                var response = await _httpClient.SendAsync(requestMessage);

                //check Code, maybe response type isn't valid
                var responseCode = await response.Content.ReadAsStringAsync();
                var code = new Code
                {
                    CompanyId = companyId,
                    PhoneNumber = phoneNumber,
                    ConfirmationCode = responseCode
                };

                await _context.Codes.AddAsync(code);
            }

            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Client's exist");
        }
    }

    [HttpPost]
    [Route("confirm-client")]
    public async Task Confirm([FromBody] string phoneNumber, int companyId, string confirmationCode)
    {
        var sentCodeInfo = await _context.Codes
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.CompanyId == companyId);

        var values = new Dictionary<string, string>
        {
            { "token", $"{sentCodeInfo.ConfirmationCode}" },
            { "pin", $"{confirmationCode}" }
        };

        var content = new FormUrlEncodedContent(values);

        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                       + "/activation/checkpin"))
        {
            requestMessage.Content = content;
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
    
            await _httpClient.SendAsync(requestMessage);
        }

        var addedCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.CompanyId == companyId);
        addedCustomer.Confirmed = true;
        await _context.SaveChangesAsync();

        await OsmiCardGenerate(phoneNumber, companyId);
    }

    private async Task GenerateCustomer(string phoneNumber, int companyId)
    {
        var rnd = new Random();
        var serialNumber = (int) rnd.NextDouble() * serialNumFormat;

        var customer = new Customer
        {
            CompanyId = companyId,
            PhoneNumber = phoneNumber,
            SerialNumber = serialNumber
        };

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    private async Task<HttpResponseMessage> OsmiSendCardOnSms(int cardId, string phoneNumber)
    {
        var encoder = UrlEncoder.Create();
        var mes = encoder.Encode("Ваша карта готова");

        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Get, OsmiInformation.HostPrefix
                                                      + $"/passes/{cardId}/sms/{phoneNumber}" +
                                                      $"?message={mes}" +
                                                      "{link}&sender=OSMICARDS"))
        {
            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
            return await _httpClient.SendAsync(requestMessage);
        }
    }
}