using Domain.Models;
using FluentResults;
using Infrastructure;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands;

public sealed record GetMeQuery(string Subject, string Issuer) : IQuery<Result<User>>;

public sealed class GetMeQueryHandler(Context context) : IQueryHandler<GetMeQuery, Result<User>>
{
    public async ValueTask<Result<User>> Handle(GetMeQuery query, CancellationToken cancellationToken)
    {
        var result = await context.Users.FirstOrDefaultAsync(u => u.OpenIdIssuer == query.Issuer && u.OpenIdSubject == query.Subject, cancellationToken);
        return result is null ? Result.Fail("User not onboarded") : Result.Ok(result);
    }
}