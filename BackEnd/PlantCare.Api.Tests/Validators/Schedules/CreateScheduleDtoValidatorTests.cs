using FluentValidation.TestHelper;
using PlantCare.Api.DTOs.Schedules;
using PlantCare.Api.Validators.Schedules;
using Xunit;

namespace PlantCare.Api.Tests.Validators.Schedules;

public class CreateScheduleDtoValidatorTests
{
    private readonly CreateScheduleDtoValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Error_When_DiagnosisId_Is_Null()
    {
        var dto = ValidDto();
        dto.DiagnosisId = null;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.DiagnosisId);
    }

    [Fact]
    public void Should_Have_Error_When_DiagnosisId_Is_Zero()
    {
        var dto = ValidDto();
        dto.DiagnosisId = 0;

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.DiagnosisId);
    }

    [Fact]
    public void Should_Not_Have_Error_When_DiagnosisId_Is_Positive()
    {
        var dto = ValidDto();
        dto.DiagnosisId = 5;

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.DiagnosisId);
    }

    private static CreateScheduleDto ValidDto()
    {
        return new CreateScheduleDto
        {
            PlantId = 1,
            DiagnosisId = null,
            Tasks =
            [
                new CreateScheduleTaskDto
                {
                    Date = DateTime.UtcNow,
                    Title = "Water",
                    TaskType = "Water",
                    TaskDescription = "Water the plant"
                }
            ]
        };
    }
}
