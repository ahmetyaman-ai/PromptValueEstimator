using MediatR;
using PromptValueEstimator.Application;
using PromptValueEstimator.Application.Features.Estimator;
using PromptValueEstimator.Application.Abstractions;
using Microsoft.Extensions.Options;
using PromptValueEstimator.Infrastructure.Serpstat;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Starting PromptValueEstimator API...");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Current Time (UTC): {DateTime.UtcNow}");

// ============================
// 🔹 1. Swagger + Servis Tanımları
// ============================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Prompt Value Estimator API",
        Version = "v1",
        Description = "Estimates ChatGPT prompt popularity using Serpstat keyword search data."
    });
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

builder.Services.AddScoped<IPromptEstimator, PromptEstimator>();
builder.Services.AddMemoryCache();

// ============================
// 🔹 2. Serpstat Entegrasyonu
// ============================
builder.Services.Configure<SerpstatOptions>(builder.Configuration.GetSection("Serpstat"));
builder.Services.AddHttpClient<SerpstatKeywordExpansionClient>((sp, http) =>
{
    var o = sp.GetRequiredService<IOptions<SerpstatOptions>>().Value;
    http.BaseAddress = new Uri(o.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
});
builder.Services.AddHttpClient<SerpstatKeywordVolumeProvider>((sp, http) =>
{
    var o = sp.GetRequiredService<IOptions<SerpstatOptions>>().Value;
    http.BaseAddress = new Uri(o.BaseUrl);
    http.Timeout = TimeSpan.FromSeconds(o.TimeoutSeconds);
});
builder.Services.AddScoped<IKeywordExpansionClient, SerpstatKeywordExpansionClient>();
builder.Services.AddScoped<IKeywordVolumeProvider, SerpstatKeywordVolumeProvider>();

var app = builder.Build();

// ============================
// 🔹 3. Swagger UI
// ============================
// Swagger sadece Development ortamında aktif
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PromptValueEstimator.Api v1");
        c.RoutePrefix = "swagger";
    });

    Console.WriteLine("Swagger UI is enabled (Development environment).");
}
else
{
    Console.WriteLine("Swagger UI is disabled (Production environment).");
}


// ============================
// 🔹 4. Health Check Endpoint
// ============================
var startTime = DateTimeOffset.UtcNow;
app.MapGet("/healthz", () =>
{
    var uptime = DateTimeOffset.UtcNow - startTime;
    return Results.Ok(new
    {
        status = "ok",
        startedAt = startTime,
        uptimeSeconds = (int)uptime.TotalSeconds,
        serverTimeUtc = DateTimeOffset.UtcNow
    });
})
.WithName("HealthCheck")
.WithOpenApi(op =>
{
    op.Summary = "Returns basic health information for the API.";
    op.Description = "Used by monitoring and load balancers to verify if the API is running.";
    return op;
});

// ============================
// 🔹 5. Root Info
// ============================
app.MapGet("/", () =>
{
    var asm = typeof(ApplicationAssemblyMarker).Assembly.GetName();
    return Results.Ok(new
    {
        Service = "Prompt Value Estimator API",
        Version = asm.Version?.ToString(),
        Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
        Endpoints = new[] { "GET /healthz", "POST /estimate" },
        TimeUtc = DateTimeOffset.UtcNow
    });
}).WithName("RootInfo");

// ============================
// 🔹 6. Prompt Estimate Endpoint (with examples)
// ============================
app.MapPost("/estimate", async (EstimatePromptVolumeQuery body, ISender sender) =>
{
    var result = await sender.Send(body);
    return Results.Ok(result);
})
.WithName("EstimatePromptVolume")
.WithOpenApi(op =>
{
    op.Summary = "Estimates monthly prompt frequency and related queries.";
    op.Description = "Takes a natural-language prompt and returns estimated monthly frequency, confidence score, and related prompts.";

    // Örnek Request
    op.RequestBody = new OpenApiRequestBody
    {
        Description = "Prompt estimation input example.",
        Content =
        {
            ["application/json"] = new OpenApiMediaType
            {
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["promptText"] = new Microsoft.OpenApi.Any.OpenApiString("explain blockchain in simple terms"),
                    ["languageCode"] = new Microsoft.OpenApi.Any.OpenApiString("en"),
                    ["geoTarget"] = new Microsoft.OpenApi.Any.OpenApiString("US"),
                    ["engine"] = new Microsoft.OpenApi.Any.OpenApiString("google"),
                    ["maxRelatedKeywords"] = new Microsoft.OpenApi.Any.OpenApiInteger(20),
                    ["includeTrends"] = new Microsoft.OpenApi.Any.OpenApiBoolean(false),
                    ["similarityThreshold"] = new Microsoft.OpenApi.Any.OpenApiDouble(0.7),
                    ["intentFilter"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
                }
            }
        }
    };

    // Örnek Response
    op.Responses["200"] = new OpenApiResponse
    {
        Description = "Successful prompt estimation response.",
        Content =
        {
            ["application/json"] = new OpenApiMediaType
            {
                Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["promptText"] = new Microsoft.OpenApi.Any.OpenApiString("explain blockchain in simple terms"),
                    ["estimatedMonthlyPromptVolume"] = new Microsoft.OpenApi.Any.OpenApiInteger(861),
                    ["confidenceScore"] = new Microsoft.OpenApi.Any.OpenApiDouble(0.49),
                    ["confidenceLabel"] = new Microsoft.OpenApi.Any.OpenApiString("Low"),
                    ["confidenceReasons"] = new Microsoft.OpenApi.Any.OpenApiArray
                    {
                        new Microsoft.OpenApi.Any.OpenApiString("No external volume (fallback heuristic)"),
                        new Microsoft.OpenApi.Any.OpenApiString("HighSimShare=0.75")
                    },
                    ["relatedHighVolumePrompts"] = new Microsoft.OpenApi.Any.OpenApiArray
                    {
                        new Microsoft.OpenApi.Any.OpenApiObject
                        {
                            ["text"] = new Microsoft.OpenApi.Any.OpenApiString("explain blockchain in simple"),
                            ["estimatedVolume"] = new Microsoft.OpenApi.Any.OpenApiInteger(228),
                            ["similarity"] = new Microsoft.OpenApi.Any.OpenApiDouble(0.8),
                            ["intentScore"] = new Microsoft.OpenApi.Any.OpenApiDouble(0.95)
                        }
                    },
                    ["lastUpdated"] = new Microsoft.OpenApi.Any.OpenApiString("2025-10-08T13:13:18Z")
                }
            }
        }
    };
    return op;
});

// ============================
// 🔹 7. Run
// ============================
app.Run();
