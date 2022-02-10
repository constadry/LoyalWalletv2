using System.Drawing;
using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Resources;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace LoyalWalletv2.Controllers;

public class CompanyController : BaseApiController
{
    private readonly AppDbContext _context;
    private IMapper _mapper;

    public CompanyController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [Authorize(Roles = nameof(EUserRoles.Admin))]
    [HttpGet]
    public async Task<IEnumerable<Company>> ListAsync()
    {
        return await _context.Companies.ToListAsync();
    }

    [Authorize(Roles = nameof(EUserRoles.Admin))]
    [HttpGet("get-by/name={name}")]
    public async Task<Company> GetByName(string? name)
    {
        return await _context.Companies.FirstOrDefaultAsync(c => c.Name == name) ??
                    throw new LoyalWalletException("Company not found");
    }

    [Authorize(Roles = nameof(EUserRoles.User))]
    [HttpPut]
    [Route("edit")]
    public async Task<IActionResult> UpdateCompany([FromBody] EditCompanyResource resource)
    {
        var company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == resource.CompanyId);

        if (company is null)
            return BadRequest("Company not found");

        company.MaxCountOfStamps = resource.MaxCountOfStamps;
        company.Name = resource.CompanyName;
        company.InstagramName = resource.InstagramName;
        await _context.SaveChangesAsync();

        return Ok(company);
    }

    [Authorize(Roles = nameof(EUserRoles.User))]
    [HttpPut]
    [Route("card-template/edit")]
    public async Task<Dictionary<string, object>> UpdateCardAsync([FromBody] CardOptionsResource cardOptions)
    {
        var company = await _context.Companies
                          .FirstOrDefaultAsync(c => c.Id == cardOptions.CompanyId) ??
                      throw new Exception("Company not found");

        var values = new Dictionary<string, object>
        {
            { "noSharing", "false" },
            { "limit", "-empty-" },
            { "logoText", $"{company.Name}" },
            { "description", "Основная карта" },
            { "style", "storeCard" },
            { "transitType", "-empty-" },
            {
                "values", new[]
                {
                    new
                    {
                        Label = "Количество штампов",
                        //maybe changes are affected when card yet used
                        Value = $"{0} / {company.MaxCountOfStamps}",
                        changeMsg = "ваши баллы %@",
                        hideLabel = false,
                        forExistingCards = false,
                        //key to change location on the card
                        key = "B3"
                    },
                }
            },
            {
                "barcode", new
                {
                    show = true,
                    showSignature = true,
                    message = "-serial-",
                    signature = "-serial-",
                    format = "QR",
                    encoding = "iso-8859-1"
                }
            },
            { 
                "colors", new 
                {
                    label = $"{ColorTranslator.ToHtml(Color.FromArgb(cardOptions.TextColor))}",
                    background = $"{ColorTranslator.ToHtml(Color.FromArgb(cardOptions.BackgroundColor))}",
                    foreground = "#00BBCC"
            }},
            {
            "images", new {
                strip = $"{cardOptions.LogotypeImg}",
                // "icon": "iVBORw0KGgoCD..XNSR0IArs4c6QAAAA"
                logo = "-empty-"
            }},
        };

        var serializedValues = JsonSerializer.Serialize(values);
        
        using (var requestMessage =
               new HttpRequestMessage(HttpMethod.Post, OsmiInformation.HostPrefix
                                                       + $"templates/{cardOptions.CompanyId}?edit=true"))
        {
            requestMessage.Content = new StringContent(
                serializedValues,
                Encoding.UTF8,
                "application/json");

            requestMessage.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", OsmiInformation.Token);
    
            // await _httpClient.SendAsync(requestMessage);
        }

        return values;
    }

    [Authorize(Roles = nameof(EUserRoles.Admin))]
    [HttpDelete("{id}")]
    public async Task<Company> DeleteAsync(int id)
    {
        var model = await _context.Companies.FindAsync(id) ??
                    throw new LoyalWalletException("Company not found");
        var result = _context.Companies.Remove(model);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
}