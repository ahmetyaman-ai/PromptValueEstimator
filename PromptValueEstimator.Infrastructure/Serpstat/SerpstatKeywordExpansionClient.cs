using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.Serpstat;

// Basit yanıt modeli (aboneliğe göre alan adları değişebilir)
internal sealed record SerpstatRelatedItem(string? Keyword);
internal sealed record SerpstatRelatedResponse(List<SerpstatRelatedItem>? Result);

public sealed class SerpstatKeywordExpansionClient : IKeywordExpansionClient
{
    private readonly HttpClient _http;
    private readonly SerpstatOptions _opts;
    private readonly ILogger<SerpstatKeywordExpansionClient> _log;

    public SerpstatKeywordExpansionClient(
        HttpClient http,
        IOptions<SerpstatOptions> opts,
        ILogger<SerpstatKeywordExpansionClient> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<IReadOnlyList<string>> GetRelatedKeywordsAsync(
        IEnumerable<string> seedPhrases,
        string languageCode,
        string geoTarget,
        int maxResults,
        CancellationToken ct)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Token yoksa sessizce boş dön
        if (string.IsNullOrWhiteSpace(_opts.Token))
            return Array.Empty<string>();

        // API maliyetini kontrol etmek için ilk 5 seed ile sınırla (MVP)
        foreach (var seed in seedPhrases.Where(s => !string.IsNullOrWhiteSpace(s)).Take(5))
        {
            try
            {
                // Not: Serpstat aboneliğine/versiyona göre action/endpoint değişebilir.
                // Bu örnek, /v4 endpointinde 'RelatedKeywords' aksiyonunu çağırır.
                // Örn: GET /v4/?action=RelatedKeywords&token=...&query=<seed>
                var url = $"/v4/?action=RelatedKeywords&token={_opts.Token}&query={Uri.EscapeDataString(seed)}";

                using var resp = await _http.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogWarning("Serpstat related keywords HTTP {Status} for seed '{Seed}'", resp.StatusCode, seed);
                    continue;
                }

                var data = await resp.Content.ReadFromJsonAsync<SerpstatRelatedResponse>(cancellationToken: ct);
                if (data?.Result is { Count: > 0 })
                {
                    foreach (var item in data.Result)
                    {
                        if (!string.IsNullOrWhiteSpace(item?.Keyword))
                            results.Add(item!.Keyword!.Trim());
                    }
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Serpstat related keywords error for seed '{Seed}'", seed);
            }

            if (results.Count >= maxResults) break;
        }

        return results.Take(maxResults).ToList();
    }
}
