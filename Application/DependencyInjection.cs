using Application.Services;
using Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<GroupAccessService>();
        services.AddInfrastructure();
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
    }
}