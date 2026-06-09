using FluentValidation;
using PlantCare.Api.DTOs.Users;

namespace PlantCare.Api.Validators.Users;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
    RuleFor(x => x.Name)
        .MaximumLength(100)
        .When(x => x.Name is not null);

    RuleFor(x => x.LastName)
        .MaximumLength(100)
        .When(x => x.LastName is not null);

    RuleFor(x => x.Gmail)
        .EmailAddress()
        .MaximumLength(150)
        .When(x => x.Gmail is not null);

    RuleFor(x => x.PhoneNumber)
        .MaximumLength(20)
        .When(x => x.PhoneNumber is not null);
    }
}