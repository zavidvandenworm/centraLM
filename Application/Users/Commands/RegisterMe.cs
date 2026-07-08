using Application.Services;
using Domain.Models;
using FluentResults;
using FluentValidation;
using Infrastructure;
using Mapster;
using Mediator;

namespace Application.Users.Commands;

public sealed class RegisterMeValidator : AbstractValidator<RegisterMeQuery>
{
    public RegisterMeValidator()
    {
        RuleFor(r => r.DisplayName).Length(3, 30).NotEmpty().NotNull();
        RuleFor(r => r.Biography).MaximumLength(1000);
    }
}

public sealed record RegisterMeQuery(string OpenIdSubject, string OpenIdIssuer, string DisplayName, string Biography) : ICommand<Result<User>>;

public sealed class RegisterMeQueryHandler(UserService userService, Context context) : ICommandHandler<RegisterMeQuery, Result<User>>
{
    public async ValueTask<Result<User>> Handle(RegisterMeQuery command, CancellationToken cancellationToken)
    {
        var checkUser = await userService.GetUserId(command.OpenIdSubject, command.OpenIdIssuer);
        if (checkUser.IsSuccess)
        {
            return Result.Fail("User already registered.");
        }
        var entry = context.Users.Add(command.Adapt<User>());
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok(entry.Entity);
    }
}