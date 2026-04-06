using Timekeeper.Domain.Enums;

namespace Timekeeper.Application.Models;

public sealed record PayrollPreview(
    Guid EmployeeId,
    string EmployeeName,
    PayFrequency PayFrequency,
    decimal GrossPay,
    decimal NetPay,
    decimal HoursWorked,
    decimal YtdGross,
    IReadOnlyList<PayrollTaxLine> TaxLines);

public sealed record PayrollTaxLine(
    string RuleCode,
    string RuleName,
    string Jurisdiction,
    decimal Amount,
    string Strategy,
    string ReviewStatus);
