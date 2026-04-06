using Timekeeper.Domain.Enums;

namespace Timekeeper.Domain.Entities;

public sealed class PayRun
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly CheckDate { get; set; }
    public PayRunStatus Status { get; set; } = PayRunStatus.Draft;

    public ICollection<PayRunLine> Lines { get; set; } = [];
}
