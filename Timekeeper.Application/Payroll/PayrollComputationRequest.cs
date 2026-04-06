using Timekeeper.Domain.Entities;
using Timekeeper.Domain.Enums;

namespace Timekeeper.Application.Payroll;

public sealed record PayrollComputationRequest(
    Employee Employee,
    TaxProfile TaxProfile,
    PayFrequency PayFrequency,
    decimal GrossPay,
    decimal HoursWorked,
    decimal YearToDateGrossPay,
    decimal DeferredAmount,
    DateOnly CheckDate,
    IReadOnlyCollection<TaxRule> CandidateRules);
