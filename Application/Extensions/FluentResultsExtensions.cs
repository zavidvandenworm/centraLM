using FluentResults;
using FluentValidation.Results;

namespace Application.Extensions;

public static class FluentResultsExtensions
{
    public static List<Error> ToFluentResultErrors(this List<ValidationFailure> validationFailures)
    {
        return validationFailures.Select(v => new Error(v.ErrorMessage)).ToList();
    }
}