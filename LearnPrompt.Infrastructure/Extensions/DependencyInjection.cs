using LearnPrompt.Application.Repositories;
using LearnPrompt.Infrastructure.Data;
using LearnPrompt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearnPrompt.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext zaten Program.cs'de de eklenmişse, burayı ister boş bırak ister aynı şekilde kullan
        // Tekrar ekleyeceksen aynısını yaz:
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ICourseRepository, CourseRepository>();

        return services;
    }
}
