using System.Diagnostics;
using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

[Authorize(Roles = nameof(EUserRoles.User))]
public class EmployeeController : BaseApiController
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{companyId:int}")]
    public async Task<IEnumerable<Employee>> ListAsync(int companyId)
    {
        Debug.Assert(_context.Companies != null, "_context.Companies != null");
        var company = await _context.Companies.Include(c => c.Employees)
                          .FirstOrDefaultAsync(c => c.Id == companyId) ??
                      throw new LoyalWalletException("Company not found");
        return company.Employees;
    }

    [HttpGet]
    [Route("{companyId:int}/{employeeName}&{employeeSurname}")]
    public async Task<Employee> GetByName(int companyId, string? employeeName, string? employeeSurname)
    {
        var employees = await ListAsync(companyId);
        return employees.FirstOrDefault(e => e.Name == employeeName && e.Surname == employeeSurname) ??
               throw new LoyalWalletException("Employee not found");
    }

    [HttpPost]
    public async Task<Employee> CreateAsync([FromBody] Employee employee)
    {
        Debug.Assert(_context.Employees != null, "_context.Employees != null");
        var result = await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    [HttpPut]
    public async Task<Employee> UpdateAsync([FromBody] Employee employee)
    {
        var existEmployee = await GetByName(employee.CompanyId, employee.Name, employee.Surname);
        existEmployee.Archived = employee.Archived;
        await _context.SaveChangesAsync();

        return existEmployee;
    }

    [HttpDelete("{id:int}")]
    public async Task<Employee> DeleteAsync(int id)
    {
        Debug.Assert(_context.Employees != null, "_context.Employees != null");
        var model = await _context.Employees.FindAsync(id) ??
                    throw new LoyalWalletException("Employee not found");
        var result = _context.Employees.Remove(model);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
}