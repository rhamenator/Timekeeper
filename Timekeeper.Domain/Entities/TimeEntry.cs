namespace Timekeeper.Domain.Entities;

public sealed class TimeEntry
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateOnly WorkDate { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    public Employee? Employee { get; set; }
}
