using NetTopologySuite.Geometries;
using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.Models.Farms;

public class Farm
{
    public int FarmId { get; set; }

    public int WorkspaceId { get; set; }

    public Workspace Workspace { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public Geometry? Boundary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Plot> Plots { get; set; } = new List<Plot>();
}
