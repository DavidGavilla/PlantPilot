using FluentValidation;
using PlantCare.Api.DTOs.Schedules;

namespace PlantCare.Api.Validators.Schedules;

public class UpdateScheduleTaskDtoValidator : AbstractValidator<UpdateScheduleTaskDto>
{
    public UpdateScheduleTaskDtoValidator()
    {
        RuleFor(x => x.IsCompleted)
            .NotNull();
    }
}