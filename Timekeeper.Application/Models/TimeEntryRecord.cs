namespace Timekeeper.Application.Models;

public sealed record TimeEntryRecord(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    DateOnly WorkDate,
    decimal HoursWorked,
    decimal OvertimeHours,
    string ProjectCode,
    string Notes);
