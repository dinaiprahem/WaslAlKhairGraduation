using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Mappings;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories;
using WaslAlkhair.Api.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Add DataBase
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(builder.Configuration["ConnictionString:DefaultSQLConnection"]));


//Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>().
    AddEntityFrameworkStores<AppDbContext>();


// Add JWT Authentication
JWTmodel? jwtOptions = builder.Configuration.GetSection("jwt").Get<JWTmodel>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

    };
});
//AutoMapper
builder.Services.AddAutoMapper (typeof(AppUserProfile).Assembly);

//Repositeries
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<APIResponse>();
builder.Services.AddScoped<JWTmodel>();

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
