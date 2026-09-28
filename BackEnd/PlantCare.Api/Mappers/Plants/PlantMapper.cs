using PlantCare.Api.DTOs.Plants;
using PlantCare.Api.Models.Plants;

namespace PlantCare.Api.Mappers;

public static class PlantMapper
{
    // Plant -> PlantDto
    public static PlantDto ToDto(Plant plant)
    {
        return new PlantDto
        {
            PlantId = plant.PlantId,
            UserId = plant.UserId,
            WorkspaceId = plant.WorkspaceId,
            PlotId = plant.PlotId,
            PlotName = plant.Plot?.Name,
            FarmId = plant.Plot?.FarmId,
            FarmName = plant.Plot?.Farm?.Name,
            Name = plant.Name,
            ScientificName = plant.ScientificName,
            DateAdded = plant.DateAdded,
            SoilMoistureLevel = plant.SoilMoistureLevel,
            IsArchived = plant.IsArchived,
            ArchivedAt = plant.ArchivedAt
        };
    }

    // IEnumerable<Plant> -> List<PlantDto>
    public static List<PlantDto> ToDtoList(IEnumerable<Plant> plants)
    {
        return plants.Select(ToDto).ToList();
    }

    // CreatePlantDto -> Plant
    // UserId/WorkspaceId are NOT set here — the service sets them from the route parameters, never
    // from client input.
    public static Plant ToModel(CreatePlantDto dto)
    {
        return new Plant
        {
            Name = dto.Name,
            ScientificName = dto.ScientificName,
            DateAdded = DateTime.UtcNow,
            SoilMoistureLevel = dto.SoilMoistureLevel,
            PlotId = dto.PlotId
        };
    }

    // UpdatePlantDto -> Existing Plant
    public static void UpdateModel(Plant plant, UpdatePlantDto dto)
    {
        plant.Name = dto.Name;
        plant.ScientificName = dto.ScientificName;
        plant.SoilMoistureLevel = dto.SoilMoistureLevel;
        plant.PlotId = dto.PlotId;
    }
}
