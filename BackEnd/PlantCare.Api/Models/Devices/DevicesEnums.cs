namespace PlantCare.Api.Models.Devices;

public enum DeviceType
{
    MoistureSensor,
    Camera,
    Pump,
    WeatherSensor,
    SolarController,
    GpsTracker,
    Custom
}

public enum DeviceCommandType
{
    Start,
    Stop,
    Restart,
    TurnOn,
    TurnOff,
    Water,
    TakePhoto,
    ReadSensor,
    Sleep,
    WakeUp,
    Ping,
    Custom
}

public enum CommandStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}

public enum TriggerType
{
    Manual,
    Automatic,
    Scheduled
}

public enum ReadingType
{
    Temperature,
    AirHumidity,
    SoilMoisture,
    LightIntensity,
    WaterLevel,
    BatteryLevel,
    Latitude,
    Longitude,
    Custom
}

public enum ReadingUnit
{
    Celsius,
    Percentage,
    Volts,
    Milliliters,
    Lux,
    DecimalDegrees,
    Custom
}