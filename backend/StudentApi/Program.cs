using Microsoft.AspNetCore.Authentication.JwtBearer;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.OpenApi.Models;
using StudentApi.Application;
using StudentApi.Infrastructure;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

var keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUrl"];

if (!string.IsNullOrWhiteSpace(keyVaultUrl))
{
    if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out var vaultUri) ||
        vaultUri.Scheme != Uri.UriSchemeHttps)
    {
        throw new InvalidOperationException(
            "AzureKeyVault:VaultUrl must be a valid HTTPS URL.");
    }

    var credentialType =
        Type.GetType("Azure.Identity.DefaultAzureCredential, Azure.Identity")
        ?? throw new InvalidOperationException(
            "Azure.Identity.DefaultAzureCredential is unavailable.");

    var optionsType =
        Type.GetType("Azure.Identity.DefaultAzureCredentialOptions, Azure.Identity")
        ?? throw new InvalidOperationException(
            "Azure.Identity.DefaultAzureCredentialOptions is unavailable.");

    var options = Activator.CreateInstance(optionsType)
        ?? throw new InvalidOperationException(
            "Unable to create DefaultAzureCredentialOptions.");

    var credential = (Azure.Core.TokenCredential)(
        Activator.CreateInstance(
            credentialType,
            new object[] { options })
        ?? throw new InvalidOperationException(
            "Unable to create DefaultAzureCredential."));

    var secretClient = new SecretClient(
        vaultUri,
        credential);

    builder.Configuration.AddAzureKeyVault(
        secretClient,
        new KeyVaultSecretManager());
}

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Student API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter a JWT token."
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>()?.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.").ExecuteAsync(context);
    });
});

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
