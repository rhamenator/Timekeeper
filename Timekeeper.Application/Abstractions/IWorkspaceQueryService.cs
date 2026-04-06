using Timekeeper.Application.Models;

namespace Timekeeper.Application.Abstractions;

public interface IWorkspaceQueryService
{
    Task<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeCard>> GetEmployeesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeEntryRecord>> GetRecentTimeEntriesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxRuleRecord>> GetTaxRulesAsync(CancellationToken cancellationToken = default);
    Task<PayrollPreview> GetPayrollPreviewAsync(Guid? employeeId = null, CancellationToken cancellationToken = default);
}
