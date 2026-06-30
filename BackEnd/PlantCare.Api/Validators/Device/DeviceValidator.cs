using System.Linq.Expressions;
using FluentValidation;
using PlantCare.Api.DTOs.Devices;

namespace PlantCare.Api.Validators.Devices;

public abstract class DeviceValidator<T> : AbstractValidator<T>
    where T : class
{
    protected void ValidateName(Expression<Func<T, string>> property)
    {
        RuleFor(property)
            .NotEmpty()
            .MaximumLength(100);
    }

    protected void ValidateDescription(Expression<Func<T, string?>> property)
    {
        RuleFor(property)
            .MaximumLength(500);
    }
}

public class CreateDeviceDtoValidator : DeviceValidator<CreateDeviceDto>
{
    public CreateDeviceDtoValidator()
    {
        ValidateName(x => x.Name);
        ValidateDescription(x => x.Description);

        RuleFor(x => x.DeviceType)
            .IsInEnum();
    }
}

public class UpdateDeviceDtoValidator : DeviceValidator<UpdateDeviceDto>
{
    public UpdateDeviceDtoValidator()
    {
        ValidateName(x => x.Name);
        ValidateDescription(x => x.Description);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Longitude.HasValue);
    }
}