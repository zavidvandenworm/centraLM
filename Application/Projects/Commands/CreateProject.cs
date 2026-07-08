using Domain.Models;
using FluentResults;
using Mediator;

namespace Application.Projects.Commands;

public sealed record CreateProjectCommand(string Name, string Description, string GroupId) : ICommand<Result<Project>>;

public sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, Result<Project>>
{
    public ValueTask<Result<Project>> Handle(CreateProjectCommand command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}