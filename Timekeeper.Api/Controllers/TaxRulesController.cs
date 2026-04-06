using Microsoft.AspNetCore.Mvc;
using Timekeeper.Application.Abstractions;

namespace Timekeeper.Api.Controllers;

[ApiController]
[Route("api/tax-rules")]
public sealed class TaxRulesController(IWorkspaceQueryService workspaceQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
        => Ok(await workspaceQueryService.GetTaxRulesAsync(cancellationToken));
}
