using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.DTOs.Schedules;

public class UpdateScheduleTaskDto
{
    public ScheduleTaskStatus Status { get; set; }
}