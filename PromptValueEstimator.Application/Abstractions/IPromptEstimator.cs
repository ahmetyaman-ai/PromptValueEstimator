using PromptValueEstimator.Application.Features.Estimate;
using PromptValueEstimator.Application.Features.Estimator;
using PromptValueEstimator.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    public interface IPromptEstimator
    {
        Task<PromptEstimationResult> EstimateAsync(
            EstimatePromptVolumeQuery request,
            CancellationToken cancellationToken);
    }
}
