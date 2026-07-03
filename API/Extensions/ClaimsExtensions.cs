using System.Security.Claims;
using FluentResults;

namespace API.Extensions;

public static class ClaimsExtensions
{
    public static Result<string> GetId(this ClaimsPrincipal user)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        return userId != null ? Result.Ok(userId.Value) : Result.Fail("Could not extract user id");
    }
}