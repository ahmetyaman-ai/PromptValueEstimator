using MediatR;
using PromptValueEstimator.Application.Abstractions;
using PromptValueEstimator.Application.Features.Estimator;
using PromptValueEstimator.Application.Models;

public sealed class EstimatePromptVolumeHandler
    : IRequestHandler<EstimatePromptVolumeQuery, PromptEstimationResult>
{
    private readonly IPromptEstimator _estimator;

    public EstimatePromptVolumeHandler(IPromptEstimator estimator)
    {
        _estimator = estimator;
    }

    public Task<PromptEstimationResult> Handle(
        EstimatePromptVolumeQuery request,
        CancellationToken cancellationToken)
        => _estimator.EstimateAsync(request, cancellationToken);
}
