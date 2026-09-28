# Resilience and failure recovery

Adapted from `codewithmukesh/dotnet-claude-kit`, skill `resilience` (MIT License, © 2025 Mukesh Murugan, https://github.com/codewithmukesh/dotnet-claude-kit). Reused: Polly v8 API guidance (`ResiliencePipeline`, not v7 `Policy`), `AddStandardResilienceHandler`/`AddResilienceHandler` patterns, rate limiting, and most anti-patterns. **Polly v8 and `Microsoft.Extensions.Http.Resilience` both work fine on .NET 8** (not .NET-10-locked), so no downgrade was needed there. **One example from the source was overridden below** (fallback-into-fake-success) because it conflicts with an explicit instruction for this project. Neither Polly nor these packages are installed in `PlantCare.Api.csproj` yet — add them only when a feature actually needs outbound resilience (an external AI call, a device command dispatch), not speculatively.

## Core principles

1. Polly **v8** `ResiliencePipeline`, never v7 `Policy`/`PolicyBuilder`/`ISyncPolicy`.
2. HTTP calls: `.AddStandardResilienceHandler()` on the `HttpClient` via DI — don't hand-wrap `ExecuteAsync` around every call site.
3. Every external call gets a timeout (per-attempt, innermost; total, outermost).
4. **Never turn a failure into an apparently-successful result.** The source material's own example does exactly this (a fallback returning HTTP 200 with `{"status":"degraded","data":[]}`) — **do not copy that pattern**. If a fallback is used, the caller must be able to tell the difference between "genuinely empty" and "the dependency is down": a distinct status code, an explicit `Degraded`/`IsFallback` flag in the response, or surfacing the failure instead of masking it. This applies everywhere in this codebase, not just HTTP — a background job that "succeeds" by silently skipping the work it couldn't do is the same bug.
5. Retries only on transient failures, and never on a non-idempotent operation without an idempotency guarantee — retrying a `POST` that creates a resource, or a command that waters a plant, risks duplicating the side effect.

## HTTP calls to external services (Gemini/plant-health AI, etc.)

```csharp
// Program.cs — default: retry + circuit breaker + timeout, production-ready
builder.Services.AddHttpClient<IPlantHealthAiService, GeminiPlantHealthAiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Gemini:BaseUrl"]!);
})
.AddStandardResilienceHandler();
```

Custom thresholds only when the default (3 retries, exponential backoff+jitter, 10% failure ratio/30s circuit breaker, 10s per-attempt / 30s total timeout) doesn't fit — see the source skill's `AddResilienceHandler("name", builder => ...)` pattern for per-service tuning if needed.

## Idempotency for non-idempotent operations

```csharp
// BAD — retrying a POST that creates a resource risks duplicates
builder.AddRetry(new HttpRetryStrategyOptions { MaxRetryAttempts = 5 });

// GOOD — idempotency key + retry only on truly transient statuses
httpClient.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
builder.AddRetry(new HttpRetryStrategyOptions
{
    MaxRetryAttempts = 3,
    ShouldHandle = static args => ValueTask.FromResult(
        args.Outcome.Result?.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.ServiceUnavailable)
});
```

## Background jobs / integrations (checklist, Section 9)

- **Timeouts and cancellation** — every job/handler takes and honors a `CancellationToken`; nothing runs unbounded.
- **Limited, transient-only retries** — no retry on business-rule failures (validation, not-found), only on timeouts/transient infra errors.
- **No overlapping retries** — a job that retries itself while a scheduler also re-triggers it multiplies work; pick one retry owner.
- **Idempotency/deduplication** for anything with a side effect — a stable operation id the handler can check before acting again.
- **Recovery after restart** — a job's state must be re-derivable from persisted state, not only in-memory.
- **Concurrency control across instances** — if more than one instance of the API could run the same job, use a DB-level lock/claim (e.g. an `UPDATE ... WHERE Status = 'Pending'` claim pattern), not an in-process lock.
- **Persist important job state** — don't rely on logs alone to know whether a job ran/succeeded.
- **Log failures with a real recovery path** — not just `catch { logger.LogError(...) }` and move on if the failure needs a human or a retry to actually resolve.

Don't add a message queue, outbox pattern, or circuit breaker for a job that doesn't have a demonstrated need for one — this project has none of that infrastructure today; introducing it is an architectural decision, not a default.

## Device commands and watering orders (Section 9 — specific to this schema)

`DeviceCommand` (the issued order) and `DeviceEvent` (what the device actually reports happened) are **already modeled as separate entities** in this schema (`Models/Devices/DeviceCommand.cs`, `Models/Devices/DeviceEvent.cs`) — that separation is exactly the "orden enviada vs. acción realmente ejecutada" distinction this section asks for; keep using it, don't collapse them into one record.

What the current schema does **not** yet have, and needs before any real command-dispatch logic ships:

- **A stable command identifier usable for deduplication** — `DeviceCommandId` is a DB identity, generated after insert; a client-supplied idempotency key (so retrying a "send command" HTTP call can't create two `DeviceCommand` rows for the same intent) doesn't exist yet.
- **Expiry** — no `ExpiresAt`/TTL on `DeviceCommand`. A command a device fetches long after being issued (e.g. after being offline) can be stale and should not execute.
- **Device confirmation vs. server-side assumption** — `DeviceCommand.Status` should only move to `Completed` on an actual device-reported `DeviceEvent`, never by the server assuming success after sending.

Checklist for whoever implements this feature:

1. Every command has a stable identifier the device can use to detect "I already did this."
2. Commands expire — a device must not execute a long-stale command.
3. `DeviceCommand.Status = Completed` only transitions on a real `DeviceEvent` confirmation from the device, not on send.
4. Bound duration and concurrency — a device shouldn't be able to run two watering commands at once, and a single watering run has a max duration (`DurationSeconds`/`WaterAmountMl` already exist on both entities — enforce sane bounds in the validator).
5. Invalid/stale sensor readings (`DeviceReading`) must not silently drive an automatic watering decision — validate freshness and range before acting on them.
6. **Unknown state — do not blindly resend.** If a command's confirmation is lost (network drop, device offline), the default must be "ask/verify device state" or require explicit re-confirmation, never "resend the watering command," since resending an already-executed watering command overwaters the plant. This is the single most important idempotency rule in this domain.
7. **Local stop must not depend on server connectivity** — a physical/firmware-level watering cutoff (max duration, local safety limit) needs to exist independent of whether the device can reach the backend. This codebase's backend can define the *intended* limits (`DurationSeconds`, `WaterAmountMl`) but **cannot itself guarantee the physical stop happens** — that's a firmware guarantee. Don't claim hardware safety is validated by a backend-only test or simulation; say plainly which guarantees are backend-side (recorded, bounded, deduplicated) versus which require firmware behavior that hasn't been verified here.

## Rate limiting (.NET built-in, no extra package)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", o => { o.PermitLimit = 100; o.Window = TimeSpan.FromSeconds(60); });
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails { Title = "Too many requests", Status = 429 }, ct);
    };
});
app.UseRateLimiter();
```

Use per endpoint (`.RequireRateLimiting("fixed")`) where a device or client could otherwise hammer the API — device-command endpoints are a natural candidate once implemented.

## Anti-patterns (kept/extended from source)

- Polly v7 syntax (`Policy.Handle<>()`, `WaitAndRetryAsync`) — never; v8 only.
- Manual `try/catch` resilience wrapping around every call site instead of configuring it once on the `HttpClient`/pipeline.
- Retry on a non-idempotent write without an idempotency key.
- Circuit breaker with no visibility (wire `ConfigureTelemetry`/OpenTelemetry if one is added).
- **Fallback that returns an apparently-successful result on failure** (overridden from source, see Core Principles #4).
- Resending a device command to "recover" from an unknown/unconfirmed state, instead of verifying device state first.
