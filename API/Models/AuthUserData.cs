using Application.Services;

namespace API.Models;

public sealed record AuthUser(string Id)
{
    public static async ValueTask<AuthUser?> BindAsync(HttpContext context)
    {
        var userService = context.RequestServices.GetRequiredService<UserService>();
        var user = await userService.GetUserId(context.User);
        return user.IsSuccess ? new AuthUser(user.Value) : null;
    }
}