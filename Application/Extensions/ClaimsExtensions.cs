using System.Security.Claims;

namespace Application.Extensions;

public static class ClaimsExtensions
{
    public record SubIss(string Subject, string Issuer);

    public static SubIss GetSubIss(this ClaimsPrincipal user)
    {
        var userId = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        return new SubIss(userId!.Value, userId!.Issuer);
    }
}