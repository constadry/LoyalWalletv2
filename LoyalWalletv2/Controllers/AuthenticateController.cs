using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LoyalWalletv2.Controllers;

public class AuthenticateController : BaseApiController
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthenticateController(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        User user = await _userManager.FindByNameAsync(model.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password)) return Unauthorized();
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
        User userExists = await _userManager.FindByNameAsync(model.Username);  
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
                {
                    Status = "Error",
                    Message = "User already exists!"
                });

        var user = new User
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
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

        return Ok(new Response { Status = "Success", Message = "User created successfully!" });
    }

    [HttpPost("register-admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
    {
        User userExists = await _userManager.FindByNameAsync(model.Username);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new Response
            {
                Status = "Error",
                Message = "User already exists!"
            });

        var user = new User
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
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
}  