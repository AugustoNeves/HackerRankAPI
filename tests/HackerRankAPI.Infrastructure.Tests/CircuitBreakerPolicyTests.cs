using System.Net;

using HackerRankAPI.Infrastructure.HackerNews;

using Polly.CircuitBreaker;

namespace HackerRankAPI.Infrastructure.Tests;

public sealed class CircuitBreakerPolicyTests
{
    [Fact]
    public async Task CreateCircuitBreakerPolicy_WhenConsecutiveFailuresReachThreshold_OpensCircuitAndShortCircuitsNextCall()
    {
        var options = new HackerNewsApiOptions
        {
            CircuitBreakerFailuresAllowedBeforeBreaking = 2,
            CircuitBreakerDurationSeconds = 30
        };

        var policy = DependencyInjection.CreateCircuitBreakerPolicy(options);

        var firstResponse = await policy.ExecuteAsync(() =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var secondResponse = await policy.ExecuteAsync(() =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Assert.ThrowsAsync<BrokenCircuitException<HttpResponseMessage>>(() => policy.ExecuteAsync(() =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError))));

        Assert.Equal(HttpStatusCode.InternalServerError, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, secondResponse.StatusCode);
    }
}
