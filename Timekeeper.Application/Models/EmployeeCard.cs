using Timekeeper.Domain.Enums;

namespace Timekeeper.Application.Models;

public sealed record EmployeeCard(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string Department,
    string WorkState,
    decimal HourlyRate,
    EmploymentStatus Status,
    string LocalTaxCode);
