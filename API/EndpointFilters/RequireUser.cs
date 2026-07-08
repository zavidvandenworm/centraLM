using API.Models;

namespace API.EndpointFilters;

public static class RequireUserFilter
{
    public static TBuilder RequireUser<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(async (context, next) =>
        {
            var user = context.Arguments.OfType<AuthUser>().FirstOrDefault();
            return user is null ? Results.Unauthorized() : await next(context);
        });
        return builder;
    }
}