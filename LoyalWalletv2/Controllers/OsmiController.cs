using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using AutoMapper;
using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Resources;
using Microsoft.AspNetCore.Authorization;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

public class OsmiController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;
    private const int serialNumFormat = 10000000;

    public OsmiController(
        HttpClient httpClient,
        AppDbContext context,
        ILogger<OsmiController> logger,
        IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
        _mapper = mapper;
    }

    [HttpPost]
    [Route("clients/register")]
    public async Task CustomerRegister([FromBody] SaveCustomerResource saveCustomerResource)
    {
        var customer = _mapper
            .Map<SaveCustomerResource, Customer>(saveCustomerResource);
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == customer.PhoneNumber
                                      && c.CompanyId == customer.CompanyId);

        if (existingCustomer is null)
        {
            await AddNewCustomer(customer.PhoneNumber, customer.CompanyId);
            _logger.LogInformation("Customer's added");

            await RegenCode(customer.PhoneNumber, customer.CompanyId);
            _logger.LogInformation("Code's generated");
        }
        else
        {
            if (existingCustomer.Confirmed) 
                await OsmiSendCardOnSms(
                    existingCustomer.SerialNumber,
                    existingCustomer.PhoneNumber);
            else
            {
                await RegenCode(customer.PhoneNumber, customer.CompanyId);
            }
        }
    }

    private async Task AddNewCustomer([FromBody] string phoneNumber, int companyId)
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

    private async Task RegenCode(string phoneNumber, int companyId)
    {
        var values = new Dictionary<string, string>
        {
            { "smsText", "Ваш пинкод для транзакции {pin}" },
            { "length", "4" }
        };

        var content = new FormUrlEncodedContent(values);

        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                    + $"/activation/sendpin/{phoneNumber}");
        requestMessage.Content = content;
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);

        // var response = await _httpClient.SendAsync(requestMessage);

        //check Code, maybe response type isn't valid

        // var responseSerialised = await response.Content.ReadAsStringAsync();
        // var (_, token) = JsonSerializer.Deserialize<KeyValuePair<string, string>>(responseSerialised);

        // _logger.LogInformation(responseCode);

        var code = new Code
        {
            CompanyId = companyId,
            PhoneNumber = phoneNumber,
            ConfirmationCode = "1111"
            // ConfirmationCode = token
        };

        await _context.Codes.AddAsync(code);
        await _context.SaveChangesAsync();
    }

    [HttpGet]
    [Route("clients/confirm/{phoneNumber}/{companyId:int}/{confirmationCode}")]
    public async Task<Dictionary<string, object>> Confirm(string phoneNumber, int companyId, string confirmationCode)
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
    
            // await _httpClient.SendAsync(requestMessage);
        }

        if (sentCodeInfo.ConfirmationCode != confirmationCode)
            throw new LoyalWalletException("Not valid confirmation code");

        var addedCustomer = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.CompanyId == companyId)??
                            throw new LoyalWalletException("Customer not found");
        addedCustomer.Confirmed = true;
        await _context.SaveChangesAsync();

        return await OsmiCardGenerate(phoneNumber, companyId);
    }

    private async Task<Dictionary<string, object>> OsmiCardGenerate(string phoneNumber, int companyId)
    {
        var existingCustomer = await _context.Customers.Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber && c.CompanyId == companyId);

        if (existingCustomer is null) throw new LoyalWalletException("Client isn't exist");
        if (!existingCustomer.Confirmed) throw new LoyalWalletException("Client isn't confirmed");

        var barcode = OsmiInformation.HostPrefix + $"/?serial_number={existingCustomer.SerialNumber}";

        var values = new Dictionary<string, object>
        {
            { "noSharing", "false" },
            { "values", new []
            {
                new
                {
                    Label = "Client's id", 
                    Value = $"{existingCustomer.Id}"
                },
                new
                {
                    Label = "Количество штампов",
                    Value = $"{existingCustomer.CountOfStamps} / {existingCustomer.Company.MaxCountOfStamps}"
                },
                new
                {
                    Label = "Номер телефона",
                    Value = $"{existingCustomer.PhoneNumber}"
                },
                new
                {
                    Label = "Id ресторана",
                    Value = $"{existingCustomer.CompanyId}"
                },
            }},
            { 
                "barcode", 
                new
                {
                    Message = barcode
                } 
            }
        };

        var serializedValues = JsonSerializer.Serialize(values);

        _logger.LogInformation("values: {Values}", values);

        //6tampCardMain?
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                       + $"passes/{existingCustomer.SerialNumber}" +
                                                       $"/{existingCustomer.Company.Name}?withValues=true"))
        {
            requestMessage.Content = new StringContent(
                serializedValues,
                Encoding.UTF8,
        "application/json");

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
    
            // await _httpClient.SendAsync(requestMessage);
        }

        await OsmiSendCardOnSms(
            existingCustomer.SerialNumber,
            existingCustomer.PhoneNumber);
        
        return values;
    }

    private async Task OsmiSendCardOnSms(int cardId, string phoneNumber)
    {
        var encoder = UrlEncoder.Create();
        var mes = encoder.Encode("Ваша карта готова");

        _logger.LogInformation(mes);

        //CardId (or serial number) allow to find created card
        using var requestMessage =
            new HttpRequestMessage(HttpMethod.Get, OsmiInformation.HostPrefix
                                                   + $"/passes/{cardId}/sms/{phoneNumber}" +
                                                   $"?message={mes}" +
                                                   "{link}&sender=OSMICARDS");
        requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
        // return await _httpClient.SendAsync(requestMessage);
    }

    [HttpPost]
    [Route("cards/check")]
    public async Task ScanCard([FromBody] string uri)
    {
        var uriParam = new Uri(uri);
        var serialNumberQuery = HttpUtility.ParseQueryString(uriParam.Query).Get("serial_number");
        if (!int.TryParse(serialNumberQuery, out var serialNUmber))
            throw new LoyalWalletException($"Invalid value of serial number {serialNumberQuery}");
        var existingCustomer = await _context.Customers
                                   .FirstOrDefaultAsync(c => c.SerialNumber == serialNUmber) ??
                               throw new LoyalWalletException("Customer not found");
        existingCustomer.AddStamp();
        await _context.SaveChangesAsync();
        
        var values = new Dictionary<string, object>
        {
            { "values", new []
            {
                new
                {
                    Label = "Client's id", 
                    Value = $"{existingCustomer.Id}"
                },
                new
                {
                    Label = "Количество штампов",
                    Value = $"{existingCustomer.CountOfStamps} / {existingCustomer.Company.MaxCountOfStamps}"
                },
                new
                {
                    Label = "Номер телефона",
                    Value = $"{existingCustomer.PhoneNumber}"
                },
                new
                {
                    Label = "Id ресторана",
                    Value = $"{existingCustomer.CompanyId}"
                },
            }},
        };

        var serializedValues = JsonSerializer.Serialize(values);

        _logger.LogInformation("values: {Values}", values);

        //6tampCardMain?
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Put, OsmiInformation.HostPrefix
                                                       + $"passes/{existingCustomer.SerialNumber}" +
                                                       $"/{existingCustomer.Company.Name}?push=true"))
        {
            requestMessage.Content = new StringContent(
                serializedValues,
                Encoding.UTF8,
                "application/json");

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
    
            // await _httpClient.SendAsync(requestMessage);
        }
    }
}