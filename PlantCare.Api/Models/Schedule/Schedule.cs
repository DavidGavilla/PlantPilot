using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Diagnoses;

namespace PlantCare.Api.Models.Schedules;

public class Schedule
{
    public int ScheduleId { get; set; }

    public int PlantId { get; set; }

    public Plant Plant { get; set; } = null!;

    public int DiagnosisId { get; set; }

    public Diagnosis Diagnosis { get; set; } = null!;

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public ICollection<ScheduleTask> Tasks { get; set; } = new List<ScheduleTask>();
}