using PlantCare.Api.Data;
using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Services.Interfaces.Device;
using PlantCare.Api.Mappers.Devices;
using Microsoft.EntityFrameworkCore;

namespace PlantCare.Api.Services.Device;

public class DeviceService : IDeviceService
{
    private readonly AppDbContext _context;

    public DeviceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(int userId, int deviceId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);

        return device == null ? null : device.ToDto();
    }

    public async Task<IEnumerable<DeviceDto>> GetDevicesByUserIdAsync(int userId)
    {
        var devices = await _context.Devices
            .Where(d => d.UserId == userId)
            .ToListAsync();

        return devices.Select(d => d.ToDto());
    }

    public async Task<DeviceDto> CreateDeviceAsync(int userId, CreateDeviceDto createDeviceDto)
    {
        var device = createDeviceDto.ToEntity();
        device.UserId = userId;

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();

        return device.ToDto();
    }

    public async Task<DeviceDto?> UpdateDeviceAsync(int userId, int deviceId, UpdateDeviceDto updateDeviceDto)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);

        if (device == null)
            return null;

        updateDeviceDto.UpdateEntity(device);

        await _context.SaveChangesAsync();

        return device.ToDto();
    }

    public async Task<bool> DeleteDeviceAsync(int userId, int deviceId)
    {
        var device = await _context.Devices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId);

        if (device == null)
            return false;

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return true;
    }
}