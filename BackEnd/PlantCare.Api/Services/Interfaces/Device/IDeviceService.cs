using PlantCare.Api.DTOs.Devices;

namespace PlantCare.Api.Services.Interfaces.Device;

public interface IDeviceService
{
    Task<DeviceDto?> GetDeviceByIdAsync(int userId, int deviceId);
    Task<IEnumerable<DeviceDto>> GetDevicesByUserIdAsync(int userId);
    Task<DeviceDto> CreateDeviceAsync(int userId, CreateDeviceDto createDeviceDto);
    Task<DeviceDto?> UpdateDeviceAsync(int userId, int deviceId, UpdateDeviceDto updateDeviceDto);
    Task<bool> DeleteDeviceAsync(int userId, int deviceId);
}