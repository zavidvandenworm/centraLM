using Application.Utilities;
using FluentResults;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class GroupAccessService(Context context)
{
    public async Task<Result> CanAccess(string userId, string groupId)
    {
        var targetGroup = await context.Groups
            .AsNoTracking()
            .Where(g => g.Id == groupId)
            .Select(g => new { g.Id, g.Path })
            .FirstOrDefaultAsync();

        if (targetGroup is null)
        {
            return Result.Fail("Group does not exist, or user has no access.");
        }

        var targetPath = targetGroup.Path;

        var canAccess = await context.Groups
            .AsNoTracking()
            .AnyAsync(g => g.GroupMembers.Any(gm => gm.UserId == userId) &&
                           (g.Id == groupId || targetPath.StartsWith(g.Path + PathUtilities.Separator)));

        return canAccess
            ? Result.Ok()
            : Result.Fail("Group does not exist, or user has no access.");
    }
}