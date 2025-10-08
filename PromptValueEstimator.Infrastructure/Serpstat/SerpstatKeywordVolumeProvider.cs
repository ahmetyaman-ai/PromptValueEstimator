using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Infrastructure.Serpstat
{
    public class SerpstatKeywordVolumeProvider : IKeywordVolumeProvider
    {
        private readonly HttpClient _httpClient;
        private readonly SerpstatOptions _options;

        public SerpstatKeywordVolumeProvider(HttpClient httpClient, IOptions<SerpstatOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public Task<IDictionary<string, int>> GetMonthlySearchVolumeAsync(IEnumerable<string> phrases, string languageCode, string geoTarget, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetVolumeAsync(string keyword, CancellationToken ct = default)
        {
            try
            {
                var request = new
                {
                    id = 1,
                    method = "KeywordsVolume",
                    @params = new
                    {
                        phrases = new[] { keyword },
                        token = _options.Token
                    }
                };

                var response = await _httpClient.PostAsJsonAsync("", request, ct);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadFromJsonAsync<SerpstatVolumeResponse>(cancellationToken: ct);
                var value = json?.result?.data?.FirstOrDefault().Value ?? 0;
                return value;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Serpstat] Volume lookup failed for '{keyword}': {ex.Message}");
                return 0; // fallback
            }
        }
    }

    public class SerpstatVolumeResponse
    {
        public Result? result { get; set; }

        public class Result
        {
            public Dictionary<string, int>? data { get; set; }
        }
    }
}
