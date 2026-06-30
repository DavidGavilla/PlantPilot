using PlantCare.Api.Models.Devices;
using PlantCare.Api.Models.Plants;

public class PlantDevice
{
    public int PlantDeviceId { get; set; }

    public int DeviceId { get; set; }

    public int PlantId { get; set; }

    public Device Device { get; set; } = null!;

    public Plant Plant { get; set; } = null!;
}