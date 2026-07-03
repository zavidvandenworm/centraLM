using Application.Groups.Commands;
using FluentValidation;

namespace Application.Groups.Validators;

public class CreateGroupValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupValidator()
    {
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Name).Length(1, 100).NotEmpty();
    }
}