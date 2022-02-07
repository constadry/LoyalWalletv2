using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using LoyalWalletv2.Contexts;
using LoyalWalletv2.Domain.Models;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using LoyalWalletv2.Resources;
using LoyalWalletv2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LoyalWalletv2.Controllers;

public class AuthenticateController : BaseApiController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthenticateController> _logger;
    private readonly IEmailService _emailService;

    public AuthenticateController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        AppDbContext context,
        IMapper mapper,
        ILogger<AuthenticateController> logger,
        IEmailService emailService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _emailService = emailService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        ApplicationUser user = await _userManager.FindByNameAsync(model.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return Unauthorized();
        IList<string> userRoles = await _userManager.GetRolesAsync(user);

        var authClaims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        authClaims.
            AddRange(userRoles.
                Select(userRole => 
                    new Claim(ClaimTypes.Role, userRole)));

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

        var token = new JwtSecurityToken(
            _configuration["JWT:ValidIssuer"],
            _configuration["JWT:ValidAudience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),  
            expiration = token.ValidTo
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterModel model)
    {
        try
        {
            ApplicationUser userExists = await _userManager.FindByNameAsync(model.Email);  
            if (userExists != null)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "User already exists!"
                    });
            
            var newCompany = await CreateCompanyAsync(
                new SaveCompanyResource
                {
                    Name = "J"
                });

            var user = new ApplicationUser
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                CompanyId = newCompany.Id
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "User creation failed! Please check user details and try again."
                    });

            if (!await _roleManager.RoleExistsAsync(nameof(EUserRoles.User)))
                await _roleManager.CreateAsync(new IdentityRole(nameof(EUserRoles.User)));

            if (await _roleManager.RoleExistsAsync(nameof(EUserRoles.User)))
                await _userManager.AddToRoleAsync(user, nameof(EUserRoles.User));
            
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Action(
                "ConfirmEmail",
                "Authenticate",
                new
                { user.Id, code },
                HttpContext.Request.Scheme);
            await _emailService.SendEmailAsync(model.Email, "Confirm your account",
                $"Подтвердите регистрацию, перейдя по ссылке: <a href='{callbackUrl}'>link</a>");

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred when registering user {Message}", e.Message);
            throw;
        }
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        ApplicationUser userExists = await _userManager.FindByNameAsync(model.Email);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
            {
                Status = "Error",
                Message = "User already exists!"
            });

        var user = new ApplicationUser
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Email
        };

        IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again."
                });

        if (!await _roleManager.RoleExistsAsync(nameof(EUserRoles.Admin)))
            await _roleManager.CreateAsync(new IdentityRole(nameof(EUserRoles.Admin)));
        if (!await _roleManager.RoleExistsAsync(nameof(EUserRoles.User)))
            await _roleManager.CreateAsync(new IdentityRole(nameof(EUserRoles.User)));

        if (await _roleManager.RoleExistsAsync(nameof(EUserRoles.Admin)))
            await _userManager.AddToRoleAsync(user, nameof(EUserRoles.Admin));

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId == null || code == null)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User confirmation failed! Please check user details and try again."
                });

        var userExist = await _userManager.FindByIdAsync(userId);

        if (userExist == null)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User not found."
                });

        var result = await _userManager.ConfirmEmailAsync(userExist, code);

        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User confirmation failed! Please check user details and try again."
                });

        return Ok(new Response { Status = "Success", Message = "User confirmed successfully!" });
    }

    private async Task<Company> CreateCompanyAsync(SaveCompanyResource saveCompanyResource)
    {
        var model = _mapper
            .Map<SaveCompanyResource, Company>(saveCompanyResource);
        var result = await _context.Companies.AddAsync(model);
        await _context.SaveChangesAsync();

        return result.Entity;
    }
}  