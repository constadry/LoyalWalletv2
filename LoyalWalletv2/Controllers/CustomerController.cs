using System.Diagnostics;
using AutoMapper;
using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Resources;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

[Route("/api/[controller]/{companyId:int}")]
[Authorize(Roles = nameof(EUserRoles.User))]
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
    public async Task<IEnumerable<CustomerResource>> ListAsync(int companyId)
    {
        var query = await CustomerList(companyId);
        var queryResource = _mapper
            .Map<IEnumerable<Customer>, IEnumerable<CustomerResource>>(query);
        return queryResource;
    }

    [HttpGet("phone-number={phoneNumber}")]
    public async Task<CustomerResource> GetByPhoneNumber(string phoneNumber, int companyId)
    {
        var queryList = await CustomerList(companyId);
        var model = queryList.FirstOrDefault(c => c.PhoneNumber == phoneNumber) ??
                    throw new LoyalWalletException("Customer not found");

        var resultResource = _mapper.Map<Customer, CustomerResource>(model);
        return resultResource;
    }

    [HttpGet("all-cards-count")]
    public async Task<int> AllCardsCount(int companyId)
    {
        var query = await CustomerList(companyId);
        return query.Count;
    }

    [HttpGet("all-stamps-count")]
    public async Task<long> AllStampsCount(int companyId)
    {
        var query = await CustomerList(companyId);
        return query.Sum(q => q.CountOfStamps);
    }

    [HttpGet("all-presents-count")]
    public async Task<long> AllPresentsCount(int companyId)
    {
        var query = await CustomerList(companyId);
        return query.Sum(q => q.CountOfStoredPresents);
    }

    [HttpPut("take-present/{id:int}")]
    public async Task<CustomerResource> TakeAsync(int id, int companyId)
    {
        Debug.Assert(_context.Customers != null, "_context.Customers != null");
        var model = await _context.Customers.FindAsync(id) ??
                    throw new LoyalWalletException("Customer not found");
        model.AddStamp();
        await _context.SaveChangesAsync();

        var resultResource = _mapper.Map<Customer, CustomerResource>(model);
        return resultResource;
    }

    private async Task<List<Customer>> CustomerList(int companyId)
    {
        Debug.Assert(_context.Customers != null, "_context.Customers != null");
        return await _context.Customers
            .Where(c => c.CompanyId == companyId)
            .ToListAsync();
    }
}