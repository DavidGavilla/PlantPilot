namespace PlantCare.Api.DTOs.Schedules;

public class CreateScheduleDto
{
    public int PlantId { get; set; }

    public int DiagnosisId { get; set; }

    public List<CreateScheduleTaskDto> Tasks { get; set; } = new();
}