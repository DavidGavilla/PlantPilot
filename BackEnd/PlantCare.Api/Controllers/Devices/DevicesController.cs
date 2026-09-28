using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.DTOs.Devices;
using PlantCare.Api.Services.Interfaces.Device;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/devices")]
public class DevicesController : ApiControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceDto>>> GetDevices(int userId)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var devices = await _deviceService.GetDevicesByUserIdAsync(userId);

        return Ok(devices);
    }

    [HttpGet("{deviceId:int}")]
    public async Task<ActionResult<DeviceDto>> GetDevice(int userId, int deviceId)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var device = await _deviceService.GetDeviceByIdAsync(userId, deviceId);

        if (device == null)
            return NotFound("Device not found.");

        return Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<DeviceDto>> CreateDevice(
        int userId,
        CreateDeviceDto dto)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var device = await _deviceService.CreateDeviceAsync(userId, dto);

        return CreatedAtAction(
            nameof(GetDevice),
            new { userId = userId, deviceId = device.DeviceId },
            device
        );
    }

    [HttpPut("{deviceId:int}")]
    public async Task<ActionResult<DeviceDto>> UpdateDevice(
        int userId,
        int deviceId,
        UpdateDeviceDto dto)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var device = await _deviceService.UpdateDeviceAsync(userId, deviceId, dto);

        if (device == null)
            return NotFound("Device not found.");

        return Ok(device);
    }

    [HttpDelete("{deviceId:int}")]
    public async Task<IActionResult> DeleteDevice(int userId, int deviceId)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var deleted = await _deviceService.DeleteDeviceAsync(userId, deviceId);

        if (!deleted)
            return NotFound("Device not found.");

        return NoContent();
    }
}