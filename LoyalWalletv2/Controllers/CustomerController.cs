using AutoMapper;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

[Authorize(Roles = nameof(EUserRoles.Admin))]
public class CustomerController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomerController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IEnumerable<CustomerResource>> ListAsync()
    {
        var query = await _context.Customers.ToListAsync();
        var queryResource = _mapper
            .Map<IEnumerable<Customer>, IEnumerable<CustomerResource>>(query);
        return queryResource;
    }

    [HttpGet("phone-number={phoneNumber}")]
    public CustomerResource GetByPhoneNumber(string phoneNumber)
    {
        var model = _context.Customers.FirstOrDefault(c => c.PhoneNumber == phoneNumber) ??
                    throw new Exception("Customer not found");

        var resultResource = _mapper.Map<Customer, CustomerResource>(model);
        return resultResource;
    }

    [HttpGet("all-cards-count")]
    public async Task<int> AllCardsCount()
    {
        var query = await _context.Customers.ToListAsync();
        return query.Count;
    }

    [HttpGet("all-stamps-count")]
    public async Task<long> AllStampsCount()
    {
        var query = await _context.Customers.ToListAsync();
        return query.Sum(q => q.CountOfStamps);
    }

    [HttpGet("all-presents-count")]
    public async Task<long> AllPresentsCount()
    {
        var query = await _context.Customers.ToListAsync();
        return query.Sum(q => q.CountOfPresents);
    }

    [HttpPost]
    public async Task<CustomerResource> CreateAsync([FromBody] SaveCustomerResource saveCustomerResource)
    {
        var model = _mapper
            .Map<SaveCustomerResource, Customer>(saveCustomerResource);
        var result = await _context.Customers.AddAsync(model);
        await _context.SaveChangesAsync();

        var resultResource = _mapper.Map<Customer, CustomerResource>(result.Entity);
        return resultResource;
    }

    [HttpDelete("{id}")]
    public async Task<CustomerResource> DeleteAsync(int id)
    {
        var model = await _context.Customers.FindAsync(id) ??
                    throw new Exception("Customer not found");
        var result = _context.Customers.Remove(model);
        await _context.SaveChangesAsync();

        var resultResource = _mapper.Map<Customer, CustomerResource>(result.Entity);
        return resultResource;
    }

    [Authorize(Roles = nameof(EUserRoles.User))]
    [HttpPut("add-stamp/{id}")]
    public async Task<CustomerResource> AddStampAsync(int id)
    {
        var model = await _context.Customers.FindAsync(id) ??
                    throw new Exception("Customer not found");
        model.AddStamp();
        await _context.SaveChangesAsync();

        var resultResource = _mapper.Map<Customer, CustomerResource>(model);
        return resultResource;
    }
    
    [Authorize(Roles = nameof(EUserRoles.User))]
    [HttpPut("take-present/{id}")]
    public async Task<CustomerResource> TakeAsync(int id)
    {
        var model = await _context.Customers.FindAsync(id) ??
                    throw new Exception("Customer not found");
        model.AddStamp();
        await _context.SaveChangesAsync();

        var resultResource = _mapper.Map<Customer, CustomerResource>(model);
        return resultResource;
    }
}