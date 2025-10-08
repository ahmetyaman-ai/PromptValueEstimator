using MediatR;
using PromptValueEstimator.Application.Abstractions;

namespace PromptValueEstimator.Application.Features.Estimator;

public sealed class EstimatePromptVolumeHandler
    : IRequestHandler<EstimatePromptVolumeQuery, EstimatePromptVolumeResult>
{
    private readonly IPromptEstimator _estimator;

    public EstimatePromptVolumeHandler(IPromptEstimator estimator)
    {
        _estimator = estimator;
    }

    public Task<EstimatePromptVolumeResult> Handle(
        EstimatePromptVolumeQuery request, CancellationToken cancellationToken)
        => _estimator.EstimateAsync(request, cancellationToken);
}
