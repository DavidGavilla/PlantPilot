using FluentValidation;
using PlantCare.Api.DTOs.Devices;

namespace PlantCare.Api.Validators.Devices;

public class CreateDeviceReadingDtoValidator : AbstractValidator<CreateDeviceReadingDto>
{
    public CreateDeviceReadingDtoValidator()
    {
        RuleFor(x => x.PlantDeviceId)
            .GreaterThan(0);

        RuleFor(x => x.ReadingType)
            .IsInEnum();

        RuleFor(x => x.Unit)
            .IsInEnum();

        RuleFor(x => x.Value)
            .NotNull();
    }
}