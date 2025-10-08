using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.Serpstat;

// Basit yanıt modeli (paket/plan farkına göre alan adları değişebilir)
internal sealed record SerpstatVolumeItem(string? Keyword, int? Volume);
internal sealed record SerpstatVolumeResponse(List<SerpstatVolumeItem>? Result);

public sealed class SerpstatKeywordVolumeProvider : IKeywordVolumeProvider
{
    private readonly HttpClient _http;
    private readonly SerpstatOptions _opts;
    private readonly ILogger<SerpstatKeywordVolumeProvider> _log;

    public SerpstatKeywordVolumeProvider(
        HttpClient http,
        IOptions<SerpstatOptions> opts,
        ILogger<SerpstatKeywordVolumeProvider> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<IDictionary<string, int>> GetMonthlySearchVolumeAsync(
        IEnumerable<string> phrases,
        string languageCode,
        string geoTarget,
        CancellationToken ct)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // Token yoksa güvenli boş dön
        if (string.IsNullOrWhiteSpace(_opts.Token))
            return map;

        // Tekrarlı/boşları temizle, küçük batch’ler halinde iste
        var clean = phrases
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var batch in clean.Chunk(20)) // 20'şerlik gruplar
        {
            var joined = string.Join(",", batch.Select(Uri.EscapeDataString));
            // Örnek endpoint: /v4/?action=KeywordInfo&token=...&query=kw1,kw2,kw3
            var url = $"/v4/?action=KeywordInfo&token={_opts.Token}&query={joined}";

            try
            {
                using var resp = await _http.GetAsync(url, ct);
                if (!resp.IsSuccessStatusCode)
                {
                    _log.LogWarning("Serpstat volume HTTP {Status}", resp.StatusCode);
                    continue;
                }

                var data = await resp.Content.ReadFromJsonAsync<SerpstatVolumeResponse>(cancellationToken: ct);
                if (data?.Result is { Count: > 0 })
                {
                    foreach (var it in data.Result)
                    {
                        if (!string.IsNullOrWhiteSpace(it.Keyword) && it.Volume.HasValue)
                            map[it.Keyword] = Math.Max(0, it.Volume.Value);
                    }
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Serpstat volume error");
            }
        }

        return map;
    }
}
