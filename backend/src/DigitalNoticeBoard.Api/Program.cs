using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using DigitalNoticeBoard.Api.Authentication;
using DigitalNoticeBoard.Api.Middleware;
using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Application.Authentication;
using DigitalNoticeBoard.Application.Notices;
using DigitalNoticeBoard.Infrastructure;
using DigitalNoticeBoard.Infrastructure.Initialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("NoticeBoard")
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=DigitalNoticeBoard;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    jwtKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}

var jwtSettings = new JwtSettings(
    jwtKey,
    builder.Configuration["Jwt:Issuer"] ?? "DigitalNoticeBoard.Api",
    builder.Configuration["Jwt:Audience"] ?? "DigitalNoticeBoard.Web",
    builder.Configuration.GetValue("Jwt:ExpiryMinutes", 120));

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<IPasswordService, AspNetPasswordService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<NoticeService>();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDataInitializer>();
    await initializer.InitializeAsync(
        connectionString,
        builder.Configuration["DemoAccounts:AdminPassword"],
        builder.Configuration["DemoAccounts:ViewerPassword"],
        app.Lifetime.ApplicationStopping);
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/api", () => Results.Ok(new
{
    name = "Digital Notice Board API",
    status = "running"
}));

app.Run();
