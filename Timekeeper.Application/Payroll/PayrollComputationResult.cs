namespace Timekeeper.Application.Payroll;

public sealed record PayrollComputationResult(
    decimal GrossPay,
    decimal TaxableGrossPay,
    decimal NetPay,
    decimal TotalEmployeeDeductions,
    decimal EmployerContributionTotal,
    IReadOnlyList<TaxLineResult> TaxLines,
    IReadOnlyList<PayrollAdjustmentResult> AdjustmentLines);

public sealed record TaxLineResult(
    string RuleCode,
    string RuleName,
    string Jurisdiction,
    decimal Amount,
    string Strategy,
    string ReviewStatus);
