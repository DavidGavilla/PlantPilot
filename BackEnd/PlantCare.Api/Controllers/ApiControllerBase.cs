using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PlantCare.Api.Controllers;

// Shared by every controller that authorizes solely by matching the token's identity against a
// {userId:int} route parameter (PlantsController, FarmsController, WorkspacesController,
// DevicesController) — a genuinely demonstrated-need primitive, not a generic dumping ground.
public abstract class ApiControllerBase : ControllerBase
{
    // Returns a 403 result if the caller's token identity doesn't match routeUserId (or is
    // missing/unparseable); returns null if the caller may proceed. This app has no challenge-based
    // auth flow, so a plain 403 ObjectResult is used instead of Forbid() (same pattern already used
    // by PlantsController/FarmsController for non-auth authorization failures). Typed as the
    // concrete ActionResult base (not IActionResult) so it implicitly converts into any action's
    // ActionResult<T> return type via ActionResult<T>'s built-in operator.
    protected ActionResult? EnsureCallerIsUser(int routeUserId)
    {
        var callerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(callerIdClaim, out var callerId) || callerId != routeUserId)
            return StatusCode(403, "Token does not match the requested user.");

        return null;
    }
}
