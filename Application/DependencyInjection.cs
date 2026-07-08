using Application.Behaviors;
using Application.Services;
using FluentValidation;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GroupAccessService>();
        services.AddScoped<UserService>();
        services.AddInfrastructure();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
        });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
    }
}