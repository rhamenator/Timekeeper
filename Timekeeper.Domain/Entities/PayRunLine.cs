namespace Timekeeper.Domain.Entities;

public sealed class PayRunLine
{
    public Guid Id { get; set; }
    public Guid PayRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public decimal GrossPay { get; set; }
    public decimal FederalTax { get; set; }
    public decimal StateTax { get; set; }
    public decimal LocalTax { get; set; }
    public decimal EmployerTax { get; set; }
    public decimal NetPay { get; set; }

    public PayRun? PayRun { get; set; }
    public Employee? Employee { get; set; }
}
