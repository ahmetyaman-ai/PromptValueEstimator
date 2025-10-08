using System.Threading;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    /// <summary>
    /// Provides keyword search volume using external APIs (e.g., Serpstat).
    /// </summary>
    public interface IKeywordVolumeProvider
    {
        Task<int> GetVolumeAsync(string keyword, CancellationToken ct);
    }
}
