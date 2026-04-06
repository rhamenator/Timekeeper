using Timekeeper.Domain.Enums;

namespace Timekeeper.Application.Models;

public sealed record TaxRuleRecord(
    Guid Id,
    string Code,
    string Name,
    TaxJurisdictionKind JurisdictionKind,
    TaxCalculationKind CalculationKind,
    TaxRuleStatus Status,
    string RegionCode,
    decimal RatePercent,
    string SourceSystem,
    string SourceReference,
    string Notes);
