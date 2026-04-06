using Microsoft.AspNetCore.Mvc;
using Timekeeper.Application.Abstractions;

namespace Timekeeper.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public sealed class DashboardController(IWorkspaceQueryService workspaceQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await workspaceQueryService.GetDashboardAsync(cancellationToken));
}
