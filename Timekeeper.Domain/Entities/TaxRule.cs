using Timekeeper.Domain.Enums;

namespace Timekeeper.Domain.Entities;

public sealed class TaxRule
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TaxJurisdictionKind JurisdictionKind { get; set; }
    public TaxCalculationKind CalculationKind { get; set; }
    public TaxRuleStatus Status { get; set; } = TaxRuleStatus.NeedsRefresh;
    public string RegionCode { get; set; } = string.Empty;
    public decimal RatePercent { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string ParametersJson { get; set; } = "{}";
    public string SourceSystem { get; set; } = string.Empty;
    public string SourceReference { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
