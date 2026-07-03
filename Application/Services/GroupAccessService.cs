using FluentResults;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class GroupAccessService(Context context)
{
    public async Task<Result> CanAccess(string userId, string groupId)
    {
        var pathGroups = await context.Groups
            .AsNoTracking()
            .Include(g => g.GroupMembers)
            .Select(g => new
            {
                g.Id,
                g.Path,
                GroupMembers = g.GroupMembers.Select(gm => gm.UserId)
            })
            .Where(g => g.Path.Contains(groupId))
            .ToListAsync();

        var isDirectMember = pathGroups
            .Any(g => g.Id == groupId && g.GroupMembers.Any(mid => mid == userId));

        var isMemberOfParent = pathGroups.Any(g =>
        {
            var split = g.Path.Split("/");
            return split.IndexOf(groupId) > split.IndexOf(g.Id);
        });

        if (isDirectMember || isMemberOfParent)
        {
            return Result.Ok();
        }

        return isDirectMember || isMemberOfParent ? 
            Result.Ok() : Result.Fail("Group does not exist, or user has no access.");
    }
}