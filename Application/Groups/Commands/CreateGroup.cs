using Application.Services;
using Domain.Models;
using FluentResults;
using Infrastructure;
using Mapster;
using Mediator;

namespace Application.Groups.Commands;

public sealed record CreateGroupCommand(string Name, string Description, string? ParentGroupId, string UserId) : ICommand<Result<Group>>;

public sealed class CreateGroupCommandHandler(Context context, GroupAccessService groupAccessService) : ICommandHandler<CreateGroupCommand, Result<Group>>
{
    public async ValueTask<Result<Group>> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        if (command.ParentGroupId != null)
        {
            var canAccess = await groupAccessService.CanAccess(command.UserId, command.ParentGroupId);
            if (canAccess.IsFailed)
            {
                return Result.Fail<Group>(canAccess.Errors);
            }
        }

        var addGroup = command.Adapt<Group>();

        addGroup.GroupMembers.Add(new
        {
            command.UserId,
        }.Adapt<GroupMember>());
        

        var result = context.Groups.Add(command.Adapt<Group>());
        
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok(result.Entity);
    }
}