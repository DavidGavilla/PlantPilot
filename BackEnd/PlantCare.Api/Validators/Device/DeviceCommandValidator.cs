using FluentValidation;
using PlantCare.Api.DTOs.Devices;

namespace PlantCare.Api.Validators.Devices;

public class CreateDeviceCommandDtoValidator : AbstractValidator<CreateDeviceCommandDto>
{
    public CreateDeviceCommandDtoValidator()
    {
        RuleFor(x => x.PlantDeviceId)
            .GreaterThan(0);

        RuleFor(x => x.CommandType)
            .IsInEnum();

        RuleFor(x => x.DurationSeconds)
            .GreaterThan(0)
            .When(x => x.DurationSeconds.HasValue);

        RuleFor(x => x.WaterAmountMl)
            .GreaterThan(0)
            .When(x => x.WaterAmountMl.HasValue);
    }
}