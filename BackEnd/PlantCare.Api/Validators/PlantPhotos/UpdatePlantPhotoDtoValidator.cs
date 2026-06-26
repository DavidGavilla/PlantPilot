using FluentValidation;
using PlantCare.Api.DTOs.Plants;

namespace PlantCare.Api.Validators.Plants;

public class CreatePlantPhotoDtoValidator : AbstractValidator<CreatePlantPhotoDto>
{
    public CreatePlantPhotoDtoValidator()
    {
        RuleFor(x => x.PlantId)
            .GreaterThan(0);

        RuleFor(x => x.ImageUrl)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Date)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Date cannot be in the future.");
    }
}