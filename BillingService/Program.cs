using System.Text;
using BillingService.Configuration;
using BillingService.Data;
using BillingService.Repositories;
using BillingService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);


//  SERVICE REGISTRATION


builder.Services.AddControllers();
builder.Services.Configure<ServiceEndpointsOptions>(
    builder.Configuration.GetSection("ServiceEndpoints"));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMQ"));

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories & Services
builder.Services.AddScoped<BillingRepository>();
builder.Services.AddScoped<BillingService.Services.BillingService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ShiftService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PdfService>();
builder.Services.AddSingleton<RabbitMQPublisher>();
// HTTP Clients
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<ProductClient>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer YOUR_TOKEN'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


//  JWT AUTHENTICATION

var key = Encoding.UTF8.GetBytes(
    builder.Configuration["Jwt:Key"] ?? throw new Exception("JWT Key is missing"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

builder.Services.AddAuthorization();


//  BUILD APP

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    BillingSeedData.Seed(dbContext);
}


//  MIDDLEWARE PIPELINE

// Global Exception Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (error != null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                message = error.Error.Message
            });
        }
    });
});

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth Middleware
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// Run App
app.Run();
