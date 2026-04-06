using Microsoft.AspNetCore.Mvc;
using Timekeeper.Application.Abstractions;

namespace Timekeeper.Api.Controllers;

[ApiController]
[Route("api/time-entries")]
public sealed class TimeEntriesController(IWorkspaceQueryService workspaceQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await workspaceQueryService.GetRecentTimeEntriesAsync(cancellationToken));
}
