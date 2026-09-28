namespace PlantCare.Api.Models.Workspaces;

public enum WorkspaceType
{
    Personal,
    Business
}

public class Workspace
{
    public int WorkspaceId { get; set; }

    public WorkspaceType Type { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();
}
