using LoyalWalletv2.Domain.Models;

namespace LoyalWalletv2;

public static class SampleData
{
    public static async void Initialize(AppDbContext context)
    {
        var company = new Company
        {
            Name = "A",
        };
        var companies = context.Companies;
        if (!companies.Any())
            await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();
    }
}