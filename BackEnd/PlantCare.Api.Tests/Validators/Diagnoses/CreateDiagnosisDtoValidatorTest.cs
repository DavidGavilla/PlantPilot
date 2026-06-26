using FluentValidation.TestHelper;
using PlantCare.Api.DTOs.Diagnoses;
using PlantCare.Api.Validators.Diagnoses;
using Xunit;

namespace PlantCare.Api.Tests.Validators.Diagnoses;

public class CreateDiagnosisProblemDtoValidatorTests
{
    private readonly CreateDiagnosisProblemDtoValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_ProblemName_Is_Empty()
    {
        var dto = ValidDto();
        dto.ProblemName = "";

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProblemName);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Should_Have_Error_When_Probability_Is_Out_Of_Range(decimal probability)
    {
        var dto = ValidDto();
        dto.Probability = probability;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Probability);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        var dto = ValidDto();
        dto.Description = "";

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Treatment_Is_Empty()
    {
        var dto = ValidDto();
        dto.Treatment = "";

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Treatment);
    }

    [Fact] 
    public void Should_Have_Error_When_Prevention_Is_Empty()
    {
        var dto = ValidDto();
        dto.Prevention = "";

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Prevention);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Extreme")]
    [InlineData("medium")]
    public void Should_Have_Error_When_Severity_Is_Invalid(string severity)
    {
        var dto = ValidDto();
        dto.Severity = severity;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Severity);
    }

    [Theory]
    [InlineData("Low")]
    [InlineData("Medium")]
    [InlineData("High")]
    [InlineData("Critical")]
    public void Should_Not_Have_Error_When_Severity_Is_Valid(string severity)
    {
        var dto = ValidDto();
        dto.Severity = severity;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Severity);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Dto_Is_Valid()
    {
        var dto = ValidDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    private static CreateDiagnosisProblemDto ValidDto()
    {
        return new CreateDiagnosisProblemDto
        {
            ProblemName = "Underwatering",
            Probability = 0.86m,
            Description = "The plant appears dehydrated.",
            Treatment = "Water the plant deeply.",
            Prevention = "Check soil moisture regularly.",
            Severity = "Medium"
        };
    }
}