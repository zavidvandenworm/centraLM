using Application.Services;
using Application.Utilities;
using Domain.Enums;
using Domain.Models;
using FluentResults;
using FluentValidation;
using Infrastructure;
using Mapster;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Groups.Commands;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Name).NotEmpty().Length(1, 100);
    }
}

public sealed record CreateGroupCommand(string Name, string Description, string? ParentGroupId, string UserId)
    : ICommand<Result<Group>>;

public sealed class CreateGroupCommandHandler(Context context, GroupAccessService groupAccessService)
    : ICommandHandler<CreateGroupCommand, Result<Group>>
{
    public async ValueTask<Result<Group>> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        string? path = null;
        var newGroupId = Guid.NewGuid().ToString();

        if (command.ParentGroupId != null)
        {
            var canAccess = await groupAccessService.CanAccess(command.UserId, command.ParentGroupId);
            if (canAccess.IsFailed) return Result.Fail<Group>(canAccess.Errors);
            path = (await context.Groups.FirstAsync(g => g.Id == command.ParentGroupId, cancellationToken)).Path;
        }

        var newGroup = command.Adapt<Group>();
        newGroup.Id = newGroupId;
        newGroup.Path = command.ParentGroupId is null ? newGroupId : PathUtilities.AddIdToPath(path!, newGroupId);

        var result = context.Groups.Add(newGroup);

        var member = new GroupMember
        {
            UserId = command.UserId,
            GroupId = result.Entity.Id,
            GroupMemberType = GroupMemberType.Admin
        };

        context.GroupMembers.Add(member);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok(result.Entity);
    }
}