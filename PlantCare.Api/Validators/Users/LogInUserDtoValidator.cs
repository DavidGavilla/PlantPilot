using FluentValidation;
using PlantCare.Api.DTOs.Users;

namespace PlantCare.Api.Validators.Users;

public class LogInUserDtoValidator : AbstractValidator<LoginUserDto>
{
    public LogInUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(150);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches(@"[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
                .WithMessage("Password must contain at least one digit.");


    }
}