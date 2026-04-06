using Timekeeper.Domain.Enums;

namespace Timekeeper.Domain.Entities;

public sealed class Employee
{
    public Guid Id { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string WorkState { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public EmploymentStatus Status { get; set; } = EmploymentStatus.Active;
    public PayFrequency PayFrequency { get; set; } = PayFrequency.BiWeekly;

    public TaxProfile? TaxProfile { get; set; }
    public ICollection<TimeEntry> TimeEntries { get; set; } = [];
    public ICollection<PayRunLine> PayRunLines { get; set; } = [];
}
