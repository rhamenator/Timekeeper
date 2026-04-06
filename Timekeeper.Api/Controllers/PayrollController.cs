using Microsoft.AspNetCore.Mvc;
using Timekeeper.Application.Abstractions;

namespace Timekeeper.Api.Controllers;

[ApiController]
[Route("api/payroll")]
public sealed class PayrollController(IWorkspaceQueryService workspaceQueryService) : ControllerBase
{
    [HttpGet("preview")]
    public async Task<IActionResult> Preview([FromQuery] Guid? employeeId, CancellationToken cancellationToken)
        => Ok(await workspaceQueryService.GetPayrollPreviewAsync(employeeId, cancellationToken));
}
