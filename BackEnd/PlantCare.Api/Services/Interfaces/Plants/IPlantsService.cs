using PlantCare.Api.DTOs.Common;
using PlantCare.Api.DTOs.Plants;

namespace PlantCare.Api.Services.Interfaces.Plants;

public enum PlantServiceStatus
{
    Success,
    NotAMember,
    NotFound,
    Forbidden,
    ValidationError
}

// Unambiguous outcome wrapper (non-generic) — used by DeletePlantAsync, which has no payload to
// return on success.
public class PlantServiceResult
{
    public PlantServiceStatus Status { get; init; }

    public string? ErrorMessage { get; init; }

    public static PlantServiceResult Ok() => new() { Status = PlantServiceStatus.Success };
    public static PlantServiceResult NotAMember() => new() { Status = PlantServiceStatus.NotAMember };
    public static PlantServiceResult NotFound() => new() { Status = PlantServiceStatus.NotFound };
    public static PlantServiceResult Forbidden() => new() { Status = PlantServiceStatus.Forbidden };
}

public class PlantServiceResult<T> : PlantServiceResult
{
    public T? Value { get; init; }

    public static PlantServiceResult<T> Ok(T value) => new() { Status = PlantServiceStatus.Success, Value = value };
    public new static PlantServiceResult<T> NotAMember() => new() { Status = PlantServiceStatus.NotAMember };
    public new static PlantServiceResult<T> NotFound() => new() { Status = PlantServiceStatus.NotFound };
    public new static PlantServiceResult<T> Forbidden() => new() { Status = PlantServiceStatus.Forbidden };
    public static PlantServiceResult<T> Invalid(string message) => new() { Status = PlantServiceStatus.ValidationError, ErrorMessage = message };
}

public interface IPlantsService
{
    Task<PlantServiceResult<PagedResult<PlantDto>>> GetPlantsAsync(
        int userId,
        int workspaceId,
        string? search,
        int page,
        int pageSize,
        bool includeArchived,
        CancellationToken cancellationToken = default);

    Task<PlantServiceResult<PlantDto>> GetPlantByIdAsync(
        int userId,
        int workspaceId,
        int plantId,
        CancellationToken cancellationToken = default);

    Task<PlantServiceResult<PlantDto>> CreatePlantAsync(
        int userId,
        int workspaceId,
        CreatePlantDto dto,
        CancellationToken cancellationToken = default);

    Task<PlantServiceResult<PlantDto>> UpdatePlantAsync(
        int userId,
        int workspaceId,
        int plantId,
        UpdatePlantDto dto,
        CancellationToken cancellationToken = default);

    Task<PlantServiceResult> DeletePlantAsync(
        int userId,
        int workspaceId,
        int plantId,
        CancellationToken cancellationToken = default);
}
