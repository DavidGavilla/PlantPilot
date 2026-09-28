using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlantCare.Api.DTOs.Workspaces;
using PlantCare.Api.Services.Interfaces.Workspaces;

namespace PlantCare.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users/{userId:int}/workspaces")]
public class WorkspacesController : ApiControllerBase
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspacesController(IWorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkspaceMembershipDto>>> GetWorkspaces(int userId, CancellationToken cancellationToken)
    {
        if (EnsureCallerIsUser(userId) is { } denied) return denied;

        var workspaces = await _workspaceService.GetWorkspacesForUserAsync(userId, cancellationToken);

        return Ok(workspaces);
    }
}
