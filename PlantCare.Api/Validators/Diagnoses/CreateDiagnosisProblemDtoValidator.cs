using FluentValidation;
using PlantCare.Api.DTOs.Diagnoses;

namespace PlantCare.Api.Validators.Diagnoses;

public class CreateDiagnosisProblemDtoValidator : AbstractValidator<CreateDiagnosisProblemDto>
{
    public CreateDiagnosisProblemDtoValidator()
    {
        RuleFor(x => x.ProblemName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Treatment)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Prevention)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.Severity)
            .NotEmpty()
            .Must(s => new[]
            {
                "Low",
                "Medium",
                "High",
                "Critical"
            }.Contains(s))
            .WithMessage("Severity must be Low, Medium, High or Critical.");
    }
}