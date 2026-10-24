using System.Text;
using CCS.Entities.Api.Auth;
using CCS.Entities.Api.Data;
using CCS.Entities.Api.Middleware;
using CCS.Entities.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Falta la sección de configuración 'Jwt'.");

if (string.IsNullOrWhiteSpace(jwtSettings.Key) || jwtSettings.Key.Length < 32)
{
    throw new InvalidOperationException("Falta 'Jwt:Key' o tiene menos de 32 caracteres.");
}

if (jwtSettings.ExpirationMinutes <= 0)
{
    throw new InvalidOperationException("Falta 'Jwt:ExpirationMinutes' o no es mayor que 0.");
}

if (jwtSettings.ClockSkewMinutes < 0)
{
    throw new InvalidOperationException("'Jwt:ClockSkewMinutes' no puede ser negativo.");
}

var hashIterations = builder.Configuration.GetValue("PasswordHashing:IterationCount", 0);
if (hashIterations <= 0)
{
    throw new InvalidOperationException("Falta 'PasswordHashing:IterationCount' o no es mayor que 0.");
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenService>();

// Hash de contraseñas
builder.Services.AddSingleton<IPasswordHasher<AppUser>>(_ =>
    new PasswordHasher<AppUser>(Options.Create(new PasswordHasherOptions
    {
        IterationCount = hashIterations
    })));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(jwtSettings.ClockSkewMinutes)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

// Acceso a datos
builder.Services.AddSingleton<DapperContext>();

// CORS: permite al frontend Angular (web/) consumir la API
var allowedOrigins = (builder.Configuration["Cors:Origins"] ?? string.Empty)
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Falta 'Cors:Origins' en la configuración (URLs del frontend separadas por ';').");
}

builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Manejo de errores
builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // La URL base se deriva de cada petición y se le envía a Scalar: sin URLs fijas en el código.
    app.MapScalarApiReference(options => options
        .WithTitle(builder.Configuration["OpenApi:Title"] ?? "API CCS")
        .WithDynamicBaseServerUrl());

    // La raíz de la API lleva directo a la documentación interactiva.
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
