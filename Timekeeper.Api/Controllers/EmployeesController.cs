using Microsoft.AspNetCore.Mvc;
using Timekeeper.Application.Abstractions;

namespace Timekeeper.Api.Controllers;

[ApiController]
[Route("api/employees")]
public sealed class EmployeesController(IWorkspaceQueryService workspaceQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await workspaceQueryService.GetEmployeesAsync(cancellationToken));
}
