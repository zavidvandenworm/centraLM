using Application.Interfaces;
using Application.Services;
using Application.Utilities;
using Domain.Models;
using FluentResults;
using FluentValidation;
using Infrastructure;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups.Commands;

public enum GroupSortDirection
{
    Ascending,
    Descending
}

public sealed record GetGroupsQuery(
    string UserId,
    int Skip,
    int Limit,
    string? ParentGroupId,
    string? Keyword,
    GroupSortDirection SortByCreated) : IResultQuery<List<Group>>;

public sealed class GetGroupsValidator : AbstractValidator<GetGroupsQuery>
{
    public GetGroupsValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 50);
        RuleFor(x => x.Skip).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Keyword).MaximumLength(100);
        RuleFor(x => x.SortByCreated).IsInEnum();
    }
}

public sealed class GetGroupsQueryHandler(Context context, GroupAccessService groupAccessService)
    : IQueryHandler<GetGroupsQuery, Result<List<Group>>>
{
    public async ValueTask<Result<List<Group>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
    {
        var accessibleGroupIds = await GetAccessibleGroupIds(query.UserId, cancellationToken);

        if (query.ParentGroupId is null)
        {
            return await context.Groups
                .AsNoTracking()
                .Include(group => group.GroupMembers)
                .Where(group => accessibleGroupIds.Contains(group.Id))
                .SortByCreated(query.SortByCreated)
                .ApplyKeyword(query.Keyword)
                .Skip(query.Skip)
                .Take(query.Limit)
                .ToListAsync(cancellationToken);
        }

        var hasAccess = await groupAccessService.CanAccess(query.UserId, query.ParentGroupId);

        if (hasAccess.IsFailed)
        {
            return Result.Fail<List<Group>>(hasAccess.Errors);
        }

        return await context.Groups
            .AsNoTracking()
            .Include(group => group.GroupMembers)
            .Where(group => group.ParentGroupId == query.ParentGroupId)
            .SortByCreated(query.SortByCreated)
            .ApplyKeyword(query.Keyword)
            .Skip(query.Skip)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> GetAccessibleGroupIds(string userId, CancellationToken cancellationToken)
    {
        var memberPaths = context.GroupMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Select(member => member.Group.Path);

        return await context.Groups
            .AsNoTracking()
            .Where(group => memberPaths.Contains(group.Path) ||
                            memberPaths.Any(memberPath => group.Path.StartsWith(memberPath + PathUtilities.Separator)))
            .Select(group => group.Id)
            .ToListAsync(cancellationToken);
    }
}

internal static class GroupQueryExtensions
{
    public static IQueryable<Group> ApplyKeyword(this IQueryable<Group> groups, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return groups;

        var pattern = $"%{keyword.Trim()}%";
        return groups.Where(group => EF.Functions.Like(group.Name, pattern) ||
                                     EF.Functions.Like(group.Description, pattern));
    }

    public static IQueryable<Group> SortByCreated(this IQueryable<Group> groups, GroupSortDirection direction)
    {
        return direction == GroupSortDirection.Ascending
            ? groups.OrderBy(group => group.Created)
            : groups.OrderByDescending(group => group.Created);
    }
}
