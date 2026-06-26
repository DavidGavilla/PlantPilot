using FluentValidation;
using PlantCare.Api.DTOs.Schedules;

namespace PlantCare.Api.Validators.Schedules;

public class CreateScheduleDtoValidator : AbstractValidator<CreateScheduleDto>
{
    public CreateScheduleDtoValidator()
    {
        RuleFor(x => x.PlantId)
            .GreaterThan(0);

        RuleFor(x => x.DiagnosisId)
            .GreaterThan(0);

        RuleFor(x => x.Tasks)
            .NotEmpty();

        RuleForEach(x => x.Tasks)
            .SetValidator(new CreateScheduleTaskDtoValidator());
    }
}