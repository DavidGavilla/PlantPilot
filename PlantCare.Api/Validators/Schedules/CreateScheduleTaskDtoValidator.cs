using FluentValidation;
using PlantCare.Api.DTOs.Schedules;

namespace PlantCare.Api.Validators.Schedules;

public class CreateScheduleTaskDtoValidator : AbstractValidator<CreateScheduleTaskDto>
{
    public CreateScheduleTaskDtoValidator()
    {
        RuleFor(x => x.Date)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.TaskType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.TaskDescription)
            .NotEmpty()
            .MaximumLength(500);
    }
}