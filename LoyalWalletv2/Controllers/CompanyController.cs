using AutoMapper;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

public class CompanyController : BaseApiController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CompanyController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IEnumerable<CompanyResource>> ListAsync()
    {
        var query = await _context.Companies.ToListAsync();
        var queryResource = _mapper
            .Map<IEnumerable<Company>, IEnumerable<CompanyResource>>(query);
        return queryResource;
    }
}