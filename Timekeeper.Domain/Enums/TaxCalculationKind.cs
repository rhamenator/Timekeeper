namespace Timekeeper.Domain.Enums;

public enum TaxCalculationKind
{
    FlatPercentage = 0,
    WageBasePercentage = 1,
    HoursWorked = 2,
    PerPeriodCapPercentage = 3,
    LocalCeilingPercentage = 4,
    AnnualizedBracket = 5
}
