using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

[Authorize(Roles = nameof(EUserRoles.User))]
public class LocationsController : BaseApiController
{
    private readonly AppDbContext _context;

    public LocationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{companyName}")]
    public async Task<IEnumerable<Location>> ListAsync(string? companyName)
    {
        var company = await _context.Companies.Include(c => c.Locations)
                          .FirstOrDefaultAsync(c => c.Name == companyName) ??
                      throw new LoyalWalletException("Company not found");
        return company.Locations;
    }

    [HttpGet]
    [Route("{companyName}/{address}")]
    public async Task<Location> GetByName(string? companyName, string? address)
    {
        var locations = await ListAsync(companyName);
        return locations.FirstOrDefault(l => l.Address == address) ??
               throw new LoyalWalletException("Employee not found");
    }

    [HttpPost]
    public async Task<Location> CreateAsync([FromBody] Location location)
    {
        var result = await _context.Locations.AddAsync(location);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    [HttpDelete("{id:int}")]
    public async Task<Location> DeleteAsync(int id)
    {
        var model = await _context.Locations.FindAsync(id) ??
                    throw new LoyalWalletException("Company not found");
        var result = _context.Locations.Remove(model);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
}