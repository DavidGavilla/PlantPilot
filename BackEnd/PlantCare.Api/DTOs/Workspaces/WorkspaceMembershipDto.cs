using PlantCare.Api.Models.Workspaces;

namespace PlantCare.Api.DTOs.Workspaces;

public class WorkspaceMembershipDto
{
    public int WorkspaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public WorkspaceType Type { get; set; }

    public WorkspaceRole Role { get; set; }
}
