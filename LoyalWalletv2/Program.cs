using System.Text;
using LoyalWalletv2;
using LoyalWalletv2.Controllers;
using LoyalWalletv2.Domain.Models.AuthenticationModels;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.Formatting = Formatting.Indented;
    });

var connection = builder.Configuration.GetConnectionString("DefaultConnection");
var version = ServerVersion.AutoDetect(connection);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connection, version));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddHttpClient<OsmiController>();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddIdentity<User, IdentityRole>()  
    .AddEntityFrameworkStores<AppDbContext>()  
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>  
    {  
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;  
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;  
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;  
    })
    .AddJwtBearer(options =>  
    {  
        options.SaveToken = true;  
        options.RequireHttpsMetadata = false;  
        options.TokenValidationParameters = new TokenValidationParameters()  
        {  
            ValidateIssuer = true,  
            ValidateAudience = true,  
            ValidAudience = builder.Configuration["JWT:ValidAudience"],  
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],  
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]))  
        };  
    });  

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();