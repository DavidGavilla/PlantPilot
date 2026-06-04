namespace PlantCare.Api.DTOs.Schedules;

public class ScheduleDto
{
    public int ScheduleId { get; set; }

    public int PlantId { get; set; }

    public int DiagnosisId { get; set; }

    public DateTime DateAdded { get; set; }

    public List<ScheduleTaskDto> Tasks { get; set; } = new();
}