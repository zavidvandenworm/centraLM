using Application.Extensions;
using Application.Interfaces;
using Application.Services;
using Domain.Models;
using FluentResults;
using FluentValidation;
using Infrastructure;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups.Commands;

public sealed record GetGroupsQuery(string UserId, int Skip, int Limit, string? ParentId) : IResultQuery<List<Group>>;

public sealed class GetGroupsValidator : AbstractValidator<GetGroupsQuery>
{
    public GetGroupsValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
        RuleFor(x => x.Skip).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetGroupsQueryHandler(Context context, GroupAccessService groupAccessService) : IQueryHandler<GetGroupsQuery, Result<List<Group>>>
{
    public async ValueTask<Result<List<Group>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var results = context.Groups.AsNoTracking()
            .AsQueryable();

        if (query.ParentId == null)
        {
            return await results
                .Include(g => g.GroupMembers)
                .Where(g => g.ParentGroupId == null &&
                            g.GroupMembers.Any(gm => gm.UserId == query.UserId))
                .Skip(query.Skip)
                .Take(query.Limit)
                .ToListAsync(cancellationToken: cancellationToken);
        }

        var hasAccess = await groupAccessService.CanAccess(query.UserId, query.ParentId);

        if (hasAccess.IsFailed)
        {
            return Result.Fail(hasAccess.Errors);
        }

        return await results
            .Where(g => g.ParentGroupId == query.ParentId)
            .Skip(query.Skip)
            .Take(query.Limit)
            .ToListAsync(cancellationToken: cancellationToken);

    }
}