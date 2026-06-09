using FluentValidation;
using PlantCare.Api.DTOs.Plants;

namespace PlantCare.Api.Validators.Plants;

public class CreatePlantPhotoDtoValidator : AbstractValidator<CreatePlantPhotoDto>
{
    public CreatePlantPhotoDtoValidator()
    {
        RuleFor(x => x.PlantId)
            .GreaterThan(0);

        RuleFor(x => x.PhotoUrl)
            .NotEmpty()
            .MaximumLength(500);
    }
}