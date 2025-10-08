using PromptValueEstimator.Application.Features.Estimator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptValueEstimator.Application.Abstractions
{
    public interface IPromptEstimator
    {
        Task<EstimatePromptVolumeResult> EstimateAsync(EstimatePromptVolumeQuery request, CancellationToken ct);
    }
}
