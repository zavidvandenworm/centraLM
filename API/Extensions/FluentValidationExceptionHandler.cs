using FluentResults;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public sealed class FluentValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException) return false;

        await validationException.ToProblemDetails().ExecuteAsync(httpContext);
        return true;
    }
}

public static class ValidationProblemExtensions
{
    private const string ValidationFailedTitle = "One or more validation errors occurred.";

    public static IResult ToProblemDetails(this ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return Results.ValidationProblem(errors, title: ValidationFailedTitle);
    }

    public static IResult ToProblemDetails<T>(this Result<T> result)
    {
        return result.Errors.ToProblemDetails();
    }

    public static IResult ToProblemDetails(this Result result)
    {
        return result.Errors.ToProblemDetails();
    }

    private static IResult ToProblemDetails(this IReadOnlyList<IError> failures)
    {
        var fieldErrors = failures
            .Where(error => error.Metadata.TryGetValue("property", out var property) && property is string)
            .GroupBy(error => (string)error.Metadata["property"]!)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Message).ToArray());

        return fieldErrors.Count != 0
            ? Results.ValidationProblem(fieldErrors, title: ValidationFailedTitle)
            : Results.BadRequest(new ProblemDetails
            {
                Title = ValidationFailedTitle,
                Status = StatusCodes.Status400BadRequest,
                Detail = failures.FirstOrDefault()?.Message
            });
    }
}
