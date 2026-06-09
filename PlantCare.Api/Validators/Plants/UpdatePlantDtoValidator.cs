using FluentValidation;
using PlantCare.Api.DTOs.Plants;

namespace PlantCare.Api.Validators.Plants;

public class UpdatePlantDtoValidator : AbstractValidator<UpdatePlantDto>
{
    public UpdatePlantDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ScientificName)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.ScientificName));
    }
}