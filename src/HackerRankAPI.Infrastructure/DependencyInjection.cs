using HackerRankAPI.Application.Stories;
using HackerRankAPI.Infrastructure.HackerNews;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

namespace HackerRankAPI.Infrastructure;

public static class DependencyInjection
{
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(HackerNewsApiOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: options.RetryCount,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(options.RetryBaseDelayMilliseconds * Math.Pow(2, attempt - 1)));
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy(HackerNewsApiOptions options)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: options.CircuitBreakerFailuresAllowedBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds));
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services
            .AddOptions<HackerNewsApiOptions>()
            .Bind(configuration.GetSection(HackerNewsApiOptions.SectionName))
            .Validate(opt => Uri.TryCreate(opt.BaseUrl, UriKind.Absolute, out _), "HackerNewsApi.BaseUrl must be a valid absolute URL.")
            .Validate(opt => opt.BestStoryIdsCacheSeconds > 0, "BestStoryIdsCacheSeconds must be greater than 0.")
            .Validate(opt => opt.StoryCacheSeconds > 0, "StoryCacheSeconds must be greater than 0.")
            .Validate(opt => opt.RequestTimeoutSeconds > 0, "RequestTimeoutSeconds must be greater than 0.")
            .Validate(opt => opt.RetryCount >= 0, "RetryCount must be greater than or equal to 0.")
            .Validate(opt => opt.RetryBaseDelayMilliseconds > 0, "RetryBaseDelayMilliseconds must be greater than 0.")
            .Validate(opt => opt.CircuitBreakerFailuresAllowedBeforeBreaking > 1, "CircuitBreakerFailuresAllowedBeforeBreaking must be greater than 1.")
            .Validate(opt => opt.CircuitBreakerDurationSeconds > 0, "CircuitBreakerDurationSeconds must be greater than 0.")
            .ValidateOnStart();

        services.AddHttpClient<IHackerNewsGateway, HackerNewsApiClient>((serviceProvider, client) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<HackerNewsApiOptions>>()
                .Value;

            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
        })
        .AddPolicyHandler((serviceProvider, _) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<HackerNewsApiOptions>>()
                .Value;

            return CreateRetryPolicy(options);
        })
        .AddPolicyHandler((serviceProvider, _) =>
        {
            var options = serviceProvider
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<HackerNewsApiOptions>>()
                .Value;

            return CreateCircuitBreakerPolicy(options);
        });

        return services;
    }
}