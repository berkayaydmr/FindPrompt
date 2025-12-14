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
  

        services.AddScoped<ICourseRepository, CourseRepository>();

        return services;
    }
}
