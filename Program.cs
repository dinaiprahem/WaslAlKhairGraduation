using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WaslAlkhair.Api.Data;
using WaslAlkhair.Api.Helpers;
using WaslAlkhair.Api.Models;
using WaslAlkhair.Api.Repositories;
using WaslAlkhair.Api.Repositories.Interfaces;
using Microsoft.OpenApi.Models;
using WaslAlkhair.Api.Profiles;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WaslAlkhair.Api.Services;
using WaslAlkhair.Api.Mappings;
using WaslAlkhair.Api.MappingProfiles;
using Stripe;
using WaslAlkhair.Api.Services.Recommendation;
using WaslAlkhair.Api.Utilities;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              ;
    });
});

// Add services to the container.

builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
    options.EnableAnnotations();

});

//Add DataBase
builder.Services.AddDbContext<AppDbContext>(
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection")));

//stripe config 
StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
var stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"];

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Remove the email confirmation requirement for sign-in to allow password resets
    options.SignIn.RequireConfirmedEmail = false;
    
    // Configure token providers for password reset
    options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
    options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    
    // Configure password requirements if needed
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
    
    // Configure user requirements
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
JWTmodel? jwtOptions = builder.Configuration.GetSection("jwt").Get<JWTmodel>();
var googleAuthSettings = builder.Configuration.GetSection("GoogleAuth").Get<GoogleAuthSettings>();

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
        ValidateIssuer = false,
        ValidIssuer = jwtOptions.Issuer,

        ValidateAudience = false,
        ValidAudience = jwtOptions.Audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

    };
}).AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = googleAuthSettings.ClientId;
    options.ClientSecret = googleAuthSettings.ClientSecret;
    options.CallbackPath = "/signin-google"; // Redirect URI
});


//Customize the API Response for Validation Errors 

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var response = new APIResponse
        {
            StatusCode = HttpStatusCode.BadRequest,
            IsSuccess = false,
            ErrorMessages = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()
        };

        return new BadRequestObjectResult(response);
    };
});


//AutoMapper
builder.Services.AddAutoMapper(typeof(AppUserProfile));
builder.Services.AddAutoMapper(typeof(OpportunityProfile));
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddAutoMapper(typeof(AssistanceProfile));
builder.Services.AddAutoMapper(typeof(AssistanceTypeProfile));

//Repositeries
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<APIResponse>();
builder.Services.AddScoped<JWTmodel>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<ISMSService, TwilioSMSService>();
builder.Services.AddSingleton<ITokenBlacklist, TokenBlacklist>();
builder.Services.AddScoped<IOpportunityRepository, OpportunityRepository>();
builder.Services.AddScoped<IFileService, CloudinaryFileService>();
builder.Services.AddScoped<IDonationCategoryRepository, DonationCategoryRepository>();
builder.Services.AddScoped<IDonationOpportunityRepository, DonationOpportunityRepository>();
builder.Services.AddScoped<ILostItemService, LostItemService>();
builder.Services.AddScoped<ILostItemRepository, LostItemRepository>();

// Register HttpClient
builder.Services.AddHttpClient();
// Add your configuration
//builder.Services.Configure<HuggingFaceOptions>(
 //   builder.Configuration.GetSection("HuggingFace"));

//Repositery
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IOpportunityParticipationRepository, OpportunityParticipationRepository>();
builder.Services.AddScoped<IAssistanceRepository, AssistanceRepository>();
builder.Services.AddScoped<IAssistanceTypeRepository, AssistanceTypeRepository>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddScoped<IRecommendationDataService, RecommendationDataService>();
builder.Services.AddScoped<IModelTrainingService, ModelTrainingService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

builder.Services.AddHostedService<ModelRetrainingBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.UseCors("AllowAll");

//app.UseMiddleware<JwtMiddleware>();
app.UseStaticFiles(); // Enables serving static files from wwwroot
app.UseHttpsRedirection();
app.UseAuthentication(); // Enable Authentication
app.UseAuthorization();

app.UseDeveloperExceptionPage();

app.MapControllers();

app.Run();
