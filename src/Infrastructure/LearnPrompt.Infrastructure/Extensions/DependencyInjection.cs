using System;
using LearnPrompt.Application.Repositories;
using LearnPrompt.Application.Processing;
using LearnPrompt.Application.Topics;
using LearnPrompt.Infrastructure.Data;
using LearnPrompt.Infrastructure.Options;
using LearnPrompt.Infrastructure.Processing;
using LearnPrompt.Infrastructure.Processing.OpenAI;
using LearnPrompt.Infrastructure.Prompts;
using LearnPrompt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LearnPrompt.Application.Prompts;

namespace LearnPrompt.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddSingleton<IChunkingService, FixedWindowChunkingService>();
        services.AddSingleton<ITextExtractionService, TextExtractionService>();
        services.AddSingleton<IEmbeddingService, DeterministicHashEmbeddingService>();
        services.AddSingleton<IVectorStore, InMemoryVectorStore>();
        services.AddScoped<IContentProcessingService, ContentProcessingService>();
        services.AddScoped<IPromptBuilder, StructuredPromptBuilder>();
        services.Configure<OpenAIOptions>(configuration.GetSection(OpenAIOptions.SectionName));

        services.AddHttpClient<OpenAITopicExtractionService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                client.BaseAddress = new Uri(options.Endpoint);
            }
        });

        services.AddScoped<ITopicExtractionService>(sp =>
            sp.GetRequiredService<OpenAITopicExtractionService>());

        services.AddScoped<IPromptService, PromptService>();
        services.AddScoped<IPromptExecutionService, PromptExecutionService>();

        services.AddHttpClient<OpenAIChatService>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OpenAIOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.Endpoint))
            {
                client.BaseAddress = new Uri(options.Endpoint);
            }
        });

        services.AddScoped<IOpenAIChatService>(sp =>
            sp.GetRequiredService<OpenAIChatService>());

        return services;
    }
}
