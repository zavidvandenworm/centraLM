using Application.Interfaces;
using Application.Pipelines;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(message);
        var validationFailures = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var failures = validationFailures
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToList();

        if (failures.Count == 0) return await next(message, cancellationToken);

        if (TryCreateFailedResult<TResponse>(failures, out var failed)) return failed!;

        throw new ValidationException(failures);
    }

    private static bool TryCreateFailedResult<T>(IReadOnlyList<ValidationFailure> failures, out T? failed)
    {
        failed = default;

        if (!typeof(T).IsGenericType || typeof(T).GetGenericTypeDefinition() != typeof(Result<>)) return false;

        var failure = Result.Fail(failures.ToValidationErrors());
        failed = (T)typeof(Result)
            .GetMethod(nameof(Result.Fail), 1, [typeof(IEnumerable<IError>)])!
            .MakeGenericMethod(typeof(T).GetGenericArguments()[0])
            .Invoke(null, [failure.Errors])!;
        return true;
    }
}
