using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.Serpstat;

public sealed class SerpstatKeywordVolumeProvider : IKeywordVolumeProvider
{
    private readonly HttpClient _httpClient;
    private readonly SerpstatOptions _options;

    public SerpstatKeywordVolumeProvider(HttpClient httpClient, IOptions<SerpstatOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<int> GetVolumeAsync(string keyword, CancellationToken ct)
    {
        try
        {
            var requestBody = new
            {
                id = 1,
                method = "KeywordsInfo",
                @params = new
                {
                    query = keyword,
                    token = _options.Token,
                    se = "g_us" // Google US (ülke kodu)
                }
            };

            var response = await _httpClient.PostAsJsonAsync("", requestBody, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<SerpstatVolumeResponse>(cancellationToken: ct);

            // "volume" değeri result.keywords dizisinden gelir
            var volume = json?.result?.keywords?.FirstOrDefault()?.volume ?? 0;
            return volume;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Serpstat] volume lookup failed for '{keyword}': {ex.Message}");
            return 0; // fallback
        }
    }
}

// === DTO modelleri ===
public class SerpstatVolumeResponse
{
    public SerpstatResult? result { get; set; }
}

public class SerpstatResult
{
    public List<SerpstatKeyword>? keywords { get; set; }
}

public class SerpstatKeyword
{
    public string? keyword { get; set; }
    public int? volume { get; set; }
}
