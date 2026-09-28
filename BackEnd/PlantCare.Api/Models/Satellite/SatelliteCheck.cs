using PlantCare.Api.Models.Farms;

namespace PlantCare.Api.Models.Satellite;

public enum SatelliteCheckResult
{
    Success,
    NoNewImages,
    Error
}

public class SatelliteCheck
{
    public int SatelliteCheckId { get; set; }

    public int FarmId { get; set; }

    public Farm Farm { get; set; } = null!;

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

    public SatelliteCheckResult Result { get; set; }

    public string? Error { get; set; }
}
