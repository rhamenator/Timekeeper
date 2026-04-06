using Timekeeper.Domain.Enums;

namespace Timekeeper.Domain.Entities;

public sealed class TaxProfile
{
    public Guid EmployeeId { get; set; }
    public FilingStatus FederalFilingStatus { get; set; } = FilingStatus.Single;
    public int FederalExemptions { get; set; }
    public int StateExemptions { get; set; }
    public decimal AdditionalFederalWithholding { get; set; }
    public bool FederalExempt { get; set; }
    public bool StateExempt { get; set; }
    public bool LocalExempt { get; set; }
    public string StateCode { get; set; } = string.Empty;
    public string LocalTaxCode { get; set; } = string.Empty;

    public Employee? Employee { get; set; }
}
