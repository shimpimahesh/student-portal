using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudentApi.Application.Common;
using StudentApi.Infrastructure.Data;
using StudentApi.Infrastructure.Repositories;
using StudentApi.Domain.Factories;
using StudentApi.Infrastructure.Authentication;
using Microsoft.Extensions.Options;

namespace StudentApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options => options.UseSqlite(
            configuration.GetConnectionString("DefaultConnection") ?? "Data Source=students.db"));
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<StudentFactory>();
        services.AddScoped<IAuthService, AuthService>();       
        services.AddOptions<AzureAdOptions>()
            .Bind(configuration.GetSection(AzureAdOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        return services;
    }
}
