namespace PromptValueEstimator.Application.Abstractions;

public interface IKeywordVolumeProvider
{
    /// <summary>
    /// Bir veya birden fazla anahtar kelime için aylık arama hacmini döndürür.
    /// Key: keyword, Value: search volume (int)
    /// </summary>
    Task<IDictionary<string, int>> GetMonthlySearchVolumeAsync(
        IEnumerable<string> phrases,
        string languageCode,
        string geoTarget,
        CancellationToken ct);
}
