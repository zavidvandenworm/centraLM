using System.Security.Claims;
using Application.Extensions;
using Domain.Models;
using FluentResults;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class UserService(Context context)
{
    public async Task<Result<User>> GetUser(string subject, string issuer)
    {
        var result = await context.Users.FirstOrDefaultAsync(u => u.OpenIdIssuer == issuer && u.OpenIdSubject == subject);
        return result is null ? Result.Fail("App user not found") : Result.Ok(result);
    }

    public async Task<Result<string>> GetUserId(string subject, string issuer)
    {
        var result = await context.Users
            .Where(u => u.OpenIdIssuer == issuer && u.OpenIdSubject == subject)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        return result is null ? Result.Fail("App user not found") : Result.Ok(result);
    }

    public async Task<Result<string>> GetUserId(ClaimsPrincipal claims)
    {
        var subIss = claims.GetSubIss();
        var result = await context.Users
            .Where(u => u.OpenIdIssuer == subIss.Issuer && u.OpenIdSubject == subIss.Subject)
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        return result is null ? Result.Fail("App user not found") : Result.Ok(result);
    }
}