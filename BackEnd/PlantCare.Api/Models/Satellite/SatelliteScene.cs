namespace PlantCare.Api.Models.Satellite;

public class SatelliteScene
{
    public int SatelliteSceneId { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public DateTime CapturedAt { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public double ResolutionMeters { get; set; }

    public string FileReference { get; set; } = string.Empty;

    public ICollection<PlotObservation> Observations { get; set; } = new List<PlotObservation>();
}
