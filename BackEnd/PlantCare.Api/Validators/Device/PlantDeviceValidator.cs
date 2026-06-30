using FluentValidation;
using PlantCare.Api.DTOs.Devices;

namespace PlantCare.Api.Validators.Devices;

public class CreatePlantDeviceDtoValidator : AbstractValidator<CreatePlantDeviceDto>
{
    public CreatePlantDeviceDtoValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0);

        RuleFor(x => x.PlantId)
            .GreaterThan(0);
    }
}