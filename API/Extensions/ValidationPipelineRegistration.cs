using Application.Interfaces;
using Application.Pipelines;
using FluentResults;
using Mediator;

namespace API.Extensions;

public static class ValidationPipelineRegistration
{
    public static void AddResultQueryValidation(this IServiceCollection services)
    {
        var registrations = typeof(IResultQuery<>).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.GetInterfaces()
                .Where(IsResultQueryInterface)
                .Select(@interface => new
                {
                    Request = type,
                    Response = @interface.GetGenericArguments()[0]
                }))
            .Distinct();

        foreach (var registration in registrations)
        {
            services.AddScoped(
                typeof(IPipelineBehavior<,>).MakeGenericType(registration.Request, typeof(Result<>).MakeGenericType(registration.Response)),
                typeof(ValidationPipeline<,>).MakeGenericType(registration.Request, registration.Response));
        }
    }

    private static bool IsResultQueryInterface(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IResultQuery<>);
    }
}
