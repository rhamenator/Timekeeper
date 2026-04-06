namespace Timekeeper.Application.Payroll;

public sealed record PayrollComputationResult(
    decimal GrossPay,
    decimal NetPay,
    IReadOnlyList<TaxLineResult> TaxLines);

public sealed record TaxLineResult(
    string RuleCode,
    string RuleName,
    string Jurisdiction,
    decimal Amount,
    string Strategy,
    string ReviewStatus);
