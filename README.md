# HackerRankAPI

ASP.NET Core (.NET 10) REST API that returns the first _n_ Hacker News best stories, sorted by score descending.

## Run

1. Restore and build:
   - `dotnet restore`
   - `dotnet build HackerRankAPI.slnx`
2. Run the API:
   - `dotnet run --project src/HackerRankAPI.Api`
3. Call endpoint:
   - `GET /api/stories/best?n=10`
4. Explore Swagger UI:
   - `http://localhost:5272/swagger`

## Response Contract

Each story has:

- `title`
- `uri`
- `postedBy`
- `time`
- `score`
- `commentCount`

## Assumptions

- `n` defaults to `BestStories:DefaultStoriesCount` when omitted.
- `n` must be in range `1..BestStories:MaxStoriesPerRequest`.
- Only valid Hacker News items of `type=story` are returned.
- Deleted/dead/invalid stories are ignored.

## Performance Considerations

- In-memory cache for best story IDs and story details.
- Bounded parallelism for detail fetches (`MaxConcurrentStoryRequests`).
- Configurable HTTP timeout and cache TTLs.
- Inbound per-IP fixed-window rate limiting to protect this API and downstream Hacker News API.
- Polly resilience policies on Hacker News outbound calls:
   - Retry with exponential backoff (`RetryCount`, `RetryBaseDelayMilliseconds`)
   - Circuit breaker (`CircuitBreakerFailuresAllowedBeforeBreaking`, `CircuitBreakerDurationSeconds`)

## Rate Limiting

The API applies a per-IP fixed-window policy to `GET /api/stories/best`.

- Policy name: `storiesPolicy`
- Rejection status: `429 Too Many Requests`
- Rejection body: RFC7807 `ProblemDetails`
- `Retry-After` response header is returned when available

### Configuration

Configure in `src/HackerRankAPI.Api/appsettings*.json`:

```json
"RateLimiting": {
  "PermitLimit": 60,
  "WindowSeconds": 60,
  "QueueLimit": 0
}
```

- `PermitLimit`: max accepted requests per window, per client IP.
- `WindowSeconds`: fixed-window size in seconds.
- `QueueLimit`: queued requests once permits are exhausted (`0` disables queueing).

## Tests

The solution includes automated tests for application behavior and infrastructure resilience.

### Test Projects

- `tests/HackerRankAPI.Application.Tests`
  - Validates best-stories use case behavior (selection, ordering, filtering).
- `tests/HackerRankAPI.Infrastructure.Tests`
  - Validates resilience policy behavior (including circuit breaker opening after consecutive failures).

### Run All Tests

```bash
dotnet test HackerRankAPI.slnx -c Release
```

### Run Tests by Project

```bash
dotnet test tests/HackerRankAPI.Application.Tests/HackerRankAPI.Application.Tests.csproj -c Release
dotnet test tests/HackerRankAPI.Infrastructure.Tests/HackerRankAPI.Infrastructure.Tests.csproj -c Release
```

### Notes

- Circuit breaker tests are deterministic and verify that repeated failures open the circuit and short-circuit subsequent calls.
- Use `-v normal` for more detailed output:
  - `dotnet test HackerRankAPI.slnx -c Release -v normal`

## Swagger / OpenAPI

Swagger is enabled for API exploration and contract documentation.

### URLs

When running locally (default profile):

- Swagger UI: `http://localhost:5272/swagger`
- OpenAPI JSON: `http://localhost:5272/swagger/v1/swagger.json`

### What is documented

- Endpoint: `GET /api/stories/best`
- Query parameter:
  - `n` (optional): number of best stories to return
- Response schema fields:
  - `title`
  - `uri`
  - `postedBy`
  - `time`
  - `score`
  - `commentCount`
- Common response codes:
  - `200 OK`
  - `400 Bad Request` (invalid `n`)

### Quick check with HTTP file

Use `src/HackerRankAPI.Api/HackerRankAPI.Api.http`:

- `GET {{HackerRankAPI.Api_HostAddress}}/swagger/v1/swagger.json`
- `GET {{HackerRankAPI.Api_HostAddress}}/api/stories/best?n=10`

