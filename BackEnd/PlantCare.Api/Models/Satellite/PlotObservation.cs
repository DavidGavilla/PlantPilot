using PlantCare.Api.Models.Farms;

namespace PlantCare.Api.Models.Satellite;

public enum ObservationQuality
{
    Good,
    Partial,
    Invalid
}

public class PlotObservation
{
    public int PlotObservationId { get; set; }

    public int PlotId { get; set; }

    public Plot Plot { get; set; } = null!;

    public int SatelliteSceneId { get; set; }

    public SatelliteScene SatelliteScene { get; set; } = null!;

    // Null when the observation is invalid/has no usable data — never a fabricated 0 or default.
    public double? Ndvi { get; set; }

    public double ValidCoveragePercent { get; set; }

    public ObservationQuality Quality { get; set; }

    public string ProcessingVersion { get; set; } = string.Empty;
}
