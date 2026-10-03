using Application.Interfaces;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Application.Pipelines;

public class ValidationPipeline<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, Result<TResponse>>
    where TRequest : IResultQuery<TResponse>
{
    public async ValueTask<Result<TResponse>> Handle(TRequest message,
        MessageHandlerDelegate<TRequest, Result<TResponse>> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(message);
        var validationFailures = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var failures = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToList();

        if (failures.Count != 0) return Result.Fail<TResponse>(failures.ToValidationErrors());

        return await next(message, cancellationToken);
    }
}

internal static class ValidationFailureExtensions
{
    public static List<Error> ToValidationErrors(this IEnumerable<ValidationFailure> failures)
    {
        return failures
            .GroupBy(failure => failure.PropertyName)
            .Select(group => new Error(string.Join(" ", group.Select(failure => failure.ErrorMessage)))
            {
                Metadata = { ["property"] = group.Key }
            })
            .ToList();
    }
}
