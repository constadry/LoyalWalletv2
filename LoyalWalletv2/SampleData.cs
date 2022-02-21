using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Tools;
using Microsoft.AspNetCore.Identity;


namespace LoyalWalletv2;

public static class SampleData
{
    public static async Task Initialize(
        AppDbContext context,
        UserManager<ApplicationUser>? userManager,
        RoleManager<IdentityRole>? roleManager)
    {
        var company = new Company
        {
            Name = "#default_company",
        };

        var companies = context.Companies;
        if (!companies.Any())
            await context.Companies.AddAsync(company);
        await context.SaveChangesAsync();

        await RegisterAdmin(userManager, roleManager, company.Id);
    }

    public static async Task RegisterAdmin(
        UserManager<ApplicationUser>? userManager,
        RoleManager<IdentityRole>? roleManager,
        int companyId)
    {
        //Only one admin!
        if (await roleManager.RoleExistsAsync(nameof(EUserRoles.Admin)))
            return;

        var model = new RegisterModel
        {
            Email = "kostya.adrianov@gmail.com",
            Password = "Password123#"
        };

        var user = new ApplicationUser
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Email,
            CompanyId = companyId,
            EmailConfirmed = true
        };

        IdentityResult result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            throw new LoyalWalletException(
                "Admin creation failed! Please check user details and try again.");

        if (!await roleManager.RoleExistsAsync(nameof(EUserRoles.Admin)))
            await roleManager.CreateAsync(new IdentityRole(nameof(EUserRoles.Admin)));
        if (!await roleManager.RoleExistsAsync(nameof(EUserRoles.User)))
            await roleManager.CreateAsync(new IdentityRole(nameof(EUserRoles.User)));

        if (await roleManager.RoleExistsAsync(nameof(EUserRoles.Admin)))
            await userManager.AddToRoleAsync(user, nameof(EUserRoles.Admin));
    }
}