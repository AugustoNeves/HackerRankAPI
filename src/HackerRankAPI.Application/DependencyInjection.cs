using HackerRankAPI.Application.Stories;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HackerRankAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<BestStoriesOptions>()
            .Bind(configuration.GetSection(BestStoriesOptions.SectionName))
            .Validate(opt => opt.DefaultStoriesCount > 0, "DefaultStoriesCount must be greater than 0.")
            .Validate(opt => opt.MaxStoriesPerRequest > 0, "MaxStoriesPerRequest must be greater than 0.")
            .Validate(opt => opt.MaxConcurrentStoryRequests > 0, "MaxConcurrentStoryRequests must be greater than 0.")
            .ValidateOnStart();

        services.AddScoped<IBestStoriesService, BestStoriesService>();

        return services;
    }
}