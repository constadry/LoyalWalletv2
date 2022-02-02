using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoyalWalletv2.Controllers;

public class EmployeeController : BaseApiController
{
    private readonly AppDbContext _context;

    public EmployeeController(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("{companyName}")]
    public async Task<IEnumerable<Employee>> ListAsync(string? companyName)
    {
        var company = await _context.Companies.Include(c => c.Employees)
                          .FirstOrDefaultAsync(c => c.Name == companyName) ??
                      throw new LoyalWalletException("Company not found");
        return company.Employees;
    }

    [HttpGet]
    [Route("{companyName}/{employeeName}&{employeeSurname}")]
    public async Task<Employee> GetByName(string? companyName, string? employeeName, string? employeeSurname)
    {
        var employees = await ListAsync(companyName);
        return employees.FirstOrDefault(e => e.Name == employeeName && e.Surname == employeeSurname) ??
               throw new LoyalWalletException("Employee not found");
    }

    [HttpPut]
    public async Task<Employee> UpdateAsync([FromBody] Employee employee)
    {
        var company = await _context.Companies.FindAsync(employee.CompanyId) ??
                      throw new LoyalWalletException("Company not found");
        var existEmployee = await GetByName(company.Name, employee.Name, employee.Surname);
        existEmployee.Archived = employee.Archived;
        await _context.SaveChangesAsync();

        return existEmployee;
    }

    [HttpPost]
    public async Task<Employee> CreateAsync([FromBody] Employee employee)
    {
        var result = await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
    
    [HttpDelete("{id:int}")]
    public async Task<Employee> DeleteAsync(int id)
    {
        var model = await _context.Employees.FindAsync(id) ??
                    throw new Exception("Company not found");
        var result = _context.Employees.Remove(model);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
}