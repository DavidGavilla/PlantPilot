using FluentValidation.TestHelper;
using PlantCare.Api.DTOs.Schedules;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Validators.Schedules;
using Xunit;

namespace PlantCare.Api.Tests.Validators.Schedules;

public class UpdateScheduleTaskDtoValidatorTests
{
    private readonly UpdateScheduleTaskDtoValidator _validator = new();

    [Theory]
    [InlineData(ScheduleTaskStatus.Pending)]
    [InlineData(ScheduleTaskStatus.Completed)]
    [InlineData(ScheduleTaskStatus.Cancelled)]
    public void Should_Not_Have_Error_When_Status_Is_Valid(ScheduleTaskStatus status)
    {
        var dto = new UpdateScheduleTaskDto { Status = status };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Should_Have_Error_When_Status_Is_Not_A_Defined_Enum_Value()
    {
        var dto = new UpdateScheduleTaskDto { Status = (ScheduleTaskStatus)99 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
