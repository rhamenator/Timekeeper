namespace Timekeeper.Application.Payroll;

public enum PayrollAdjustmentKind
{
    PreTaxDeduction,
    PostTaxDeduction,
    EmployerContribution
}

public sealed record PayrollAdjustment(
    string Code,
    string Name,
    PayrollAdjustmentKind Kind,
    decimal FixedAmount = 0m,
    decimal RatePercent = 0m,
    decimal PerPeriodCap = 0m,
    decimal AnnualCap = 0m,
    decimal YearToDateAmount = 0m);

public sealed record PayrollAdjustmentResult(
    string Code,
    string Name,
    PayrollAdjustmentKind Kind,
    decimal Amount);
