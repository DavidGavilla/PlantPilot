namespace PlantCare.Api.Models.Devices;

public class PlantDevice
{
    public int PlantDeviceId { get; set; }

    public int DeviceId { get; set; }

    public int PlantId { get; set; }

    public Device Device { get; set; } = null!;
}