using FluentValidation;
using PlantCare.Api.DTOs.Diagnoses;

namespace PlantCare.Api.Validators.Diagnoses;

public class CreateDiagnosisDtoValidator : AbstractValidator<CreateDiagnosisDto>
{
    public CreateDiagnosisDtoValidator()
    {
        RuleFor(x => x.PlantPhotoId)
            .GreaterThan(0);

        RuleFor(x => x.HealthProbability)
            .InclusiveBetween(0, 1);

        RuleFor(x => x.AiProvider)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(1000);

        RuleFor(x => x.RawAiResponse)
            .NotEmpty();

        RuleForEach(x => x.Problems)
            .SetValidator(new CreateDiagnosisProblemDtoValidator());
    }
}