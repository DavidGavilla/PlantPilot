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
            Name = plant.Name,
            ScientificName = plant.ScientificName,
            DateAdded = plant.DateAdded
        };
    }

    // IEnumerable<Plant> -> List<PlantDto>
    public static List<PlantDto> ToDtoList(IEnumerable<Plant> plants)
    {
        return plants.Select(ToDto).ToList();
    }

    // CreatePlantDto -> Plant
    public static Plant ToModel(CreatePlantDto dto)
    {
        return new Plant
        {
            UserId = dto.UserId,
            Name = dto.Name,
            ScientificName = dto.ScientificName,
            DateAdded = DateTime.UtcNow
        };
    }

    // UpdatePlantDto -> Existing Plant
    public static void UpdateModel(Plant plant, UpdatePlantDto dto)
    {
        plant.Name = dto.Name;
        plant.ScientificName = dto.ScientificName;
    }
}