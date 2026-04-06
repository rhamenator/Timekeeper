using Microsoft.EntityFrameworkCore;
using Timekeeper.Application.Abstractions;
using Timekeeper.Application.Models;
using Timekeeper.Application.Payroll;
using Timekeeper.Domain.Entities;
using Timekeeper.Infrastructure.Persistence;

namespace Timekeeper.Infrastructure.Services;

public sealed class WorkspaceQueryService(TimekeeperDbContext dbContext, TaxRuleEngine taxRuleEngine) : IWorkspaceQueryService
{
    public async Task<DashboardSnapshot> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var employees = await dbContext.Employees.Include(item => item.TaxProfile).ToListAsync(cancellationToken);
        var timeEntries = await dbContext.TimeEntries.ToListAsync(cancellationToken);
        var payRunLines = await dbContext.PayRunLines.ToListAsync(cancellationToken);
        var rules = await dbContext.TaxRules.ToListAsync(cancellationToken);

        var currentPeriodHours = timeEntries
            .Where(item => item.WorkDate >= new DateOnly(2026, 3, 20))
            .Sum(item => item.HoursWorked + item.OvertimeHours);

        var alerts = new List<PayrollAlert>
        {
            new("Legacy tax rates detected", $"{rules.Count(item => item.Status == Domain.Enums.TaxRuleStatus.NeedsRefresh)} seeded rules are intentionally marked for refresh before production use.", "warning"),
            new("FoxPro migration path ready", "The new schema can accept imported hours, pay history, and tax metadata without carrying forward the old DBF file format.", "info"),
            new("Packaging baseline prepared", "Publish scripts target Windows executable, MSIX, macOS Intel/Apple silicon, and Linux package formats.", "success")
        };

        var grossPreview = decimal.Round(payRunLines.Sum(item => item.GrossPay), 2);
        var metrics = new[]
        {
            new MetricCard("Active People", employees.Count(item => item.Status == Domain.Enums.EmploymentStatus.Active).ToString(), "aqua"),
            new MetricCard("Current Hours", $"{currentPeriodHours:0.##}", "gold"),
            new MetricCard("Legacy Rules", rules.Count(item => item.Status == Domain.Enums.TaxRuleStatus.NeedsRefresh).ToString(), "coral"),
            new MetricCard("Posted Gross", grossPreview.ToString("$#,##0.00"), "mint")
        };

        return new DashboardSnapshot(
            employees.Count(item => item.Status == Domain.Enums.EmploymentStatus.Active),
            currentPeriodHours,
            grossPreview,
            rules.Count(item => item.Status == Domain.Enums.TaxRuleStatus.NeedsRefresh),
            metrics,
            alerts);
    }

    public async Task<IReadOnlyList<EmployeeCard>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        => await dbContext.Employees
            .Include(item => item.TaxProfile)
            .OrderBy(item => item.FullName)
            .Select(item => new EmployeeCard(
                item.Id,
                item.EmployeeNumber,
                item.FullName,
                item.Department,
                item.WorkState,
                item.HourlyRate,
                item.Status,
                item.TaxProfile == null ? string.Empty : item.TaxProfile.LocalTaxCode))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TimeEntryRecord>> GetRecentTimeEntriesAsync(CancellationToken cancellationToken = default)
        => await dbContext.TimeEntries
            .Include(item => item.Employee)
            .OrderByDescending(item => item.WorkDate)
            .ThenByDescending(item => item.HoursWorked)
            .Take(12)
            .Select(item => new TimeEntryRecord(
                item.Id,
                item.EmployeeId,
                item.Employee!.FullName,
                item.WorkDate,
                item.HoursWorked,
                item.OvertimeHours,
                item.ProjectCode,
                item.Notes))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TaxRuleRecord>> GetTaxRulesAsync(CancellationToken cancellationToken = default)
        => await dbContext.TaxRules
            .OrderBy(item => item.JurisdictionKind)
            .ThenBy(item => item.RegionCode)
            .ThenBy(item => item.Code)
            .Select(item => new TaxRuleRecord(
                item.Id,
                item.Code,
                item.Name,
                item.JurisdictionKind,
                item.CalculationKind,
                item.Status,
                item.RegionCode,
                item.RatePercent,
                item.SourceSystem,
                item.SourceReference,
                item.Notes))
            .ToListAsync(cancellationToken);

    public async Task<PayrollPreview> GetPayrollPreviewAsync(Guid? employeeId = null, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees
            .Include(item => item.TaxProfile)
            .OrderBy(item => item.FullName)
            .FirstAsync(item => !employeeId.HasValue || item.Id == employeeId.Value, cancellationToken);

        var timeEntries = await dbContext.TimeEntries
            .Where(item => item.EmployeeId == employee.Id && item.WorkDate >= new DateOnly(2026, 3, 20))
            .ToListAsync(cancellationToken);

        var ytdGross = await dbContext.PayRunLines
            .Where(item => item.EmployeeId == employee.Id)
            .SumAsync(item => item.GrossPay, cancellationToken);

        var grossPay = decimal.Round(timeEntries.Sum(item => (item.HoursWorked * employee.HourlyRate) + (item.OvertimeHours * employee.HourlyRate * 1.5m)), 2);
        var hoursWorked = timeEntries.Sum(item => item.HoursWorked + item.OvertimeHours);
        var rules = await dbContext.TaxRules.ToListAsync(cancellationToken);

        var result = taxRuleEngine.Compute(new PayrollComputationRequest(
            employee,
            employee.TaxProfile ?? throw new InvalidOperationException($"Employee {employee.FullName} is missing a tax profile."),
            employee.PayFrequency,
            grossPay,
            hoursWorked,
            ytdGross,
            0m,
            new DateOnly(2026, 4, 3),
            rules));

        return new PayrollPreview(
            employee.Id,
            employee.FullName,
            employee.PayFrequency,
            result.GrossPay,
            result.NetPay,
            hoursWorked,
            ytdGross,
            result.TaxLines.Select(item => new PayrollTaxLine(item.RuleCode, item.RuleName, item.Jurisdiction, item.Amount, item.Strategy, item.ReviewStatus)).ToList());
    }
}
