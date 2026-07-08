using Application.Interfaces;
using FluentResults;
using FluentValidation;
using Mediator;

namespace Application.Pipelines;

public class ValidationPipeline<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : IResultQuery<TResponse>
{
    public async ValueTask<Result<TResponse>> Handle(TRequest message, MessageHandlerDelegate<TRequest, Result<TResponse>> next, CancellationToken cancellationToken)
    {
        var failures = validators
            .Select(v => v.Validate(message))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Count != 0)
        {
            return Result.Fail(failures.Select(f => f.ErrorMessage));
        }

        return await next(message, cancellationToken);
    }
}