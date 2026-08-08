using Timekeeper.Application.Payroll;
using Timekeeper.Domain.Entities;
using Timekeeper.Domain.Enums;

namespace Timekeeper.Tests;

public sealed class TaxRuleEngineTests
{
    [Fact]
    public void WageBasePercentage_StopsWhenYtdExceedsBase()
    {
        var engine = new TaxRuleEngine();
        var employee = BuildEmployee();
        var profile = BuildProfile();

        var result = engine.Compute(new PayrollComputationRequest(
            employee,
            profile,
            PayFrequency.BiWeekly,
            2000m,
            80m,
            60000m,
            0m,
            new DateOnly(2026, 4, 3),
            [new TaxRule
            {
                Code = "FICA",
                Name = "FICA",
                JurisdictionKind = TaxJurisdictionKind.Federal,
                CalculationKind = TaxCalculationKind.WageBasePercentage,
                Status = TaxRuleStatus.Active,
                RegionCode = "*",
                RatePercent = 6.2m,
                EffectiveFrom = new DateOnly(2020, 1, 1),
                ParametersJson = """{"annualWageBase":57600}"""
            }]));

        Assert.Single(result.TaxLines);
        Assert.Equal(0m, result.TaxLines[0].Amount);
    }

    [Fact]
    public void LocalCeilingPercentage_TaxesOnlyRemainingCeiling()
    {
        var engine = new TaxRuleEngine();
        var employee = BuildEmployee();
        var profile = BuildProfile();
        profile.LocalTaxCode = "CITY";

        var result = engine.Compute(new PayrollComputationRequest(
            employee,
            profile,
            PayFrequency.Weekly,
            1000m,
            40m,
            950m,
            0m,
            new DateOnly(2026, 4, 3),
            [new TaxRule
            {
                Code = "CITY",
                Name = "Local",
                JurisdictionKind = TaxJurisdictionKind.Local,
                CalculationKind = TaxCalculationKind.LocalCeilingPercentage,
                Status = TaxRuleStatus.Active,
                RegionCode = "CITY",
                RatePercent = 1m,
                EffectiveFrom = new DateOnly(2020, 1, 1),
                ParametersJson = """{"annualWageBase":1000}"""
            }]));

        Assert.Single(result.TaxLines);
        Assert.Equal(0.5m, result.TaxLines[0].Amount);
    }

    [Fact]
    public void Adjustments_ApplyPreTaxPostTaxAndEmployerContributionInOrder()
    {
        var engine = new TaxRuleEngine();
        var result = engine.Compute(new PayrollComputationRequest(
            BuildEmployee(),
            BuildProfile(),
            PayFrequency.BiWeekly,
            1000m,
            80m,
            0m,
            50m,
            new DateOnly(2026, 4, 3),
            [new TaxRule
            {
                Code = "FED",
                Name = "Federal test rate",
                JurisdictionKind = TaxJurisdictionKind.Federal,
                CalculationKind = TaxCalculationKind.FlatPercentage,
                Status = TaxRuleStatus.Active,
                RegionCode = "*",
                RatePercent = 10m,
                EffectiveFrom = new DateOnly(2020, 1, 1)
            }],
            [
                new("RETIRE", "Retirement", PayrollAdjustmentKind.PreTaxDeduction, RatePercent: 5m),
                new("GARNISH", "Garnishment", PayrollAdjustmentKind.PostTaxDeduction, FixedAmount: 100m),
                new("MATCH", "Employer match", PayrollAdjustmentKind.EmployerContribution, RatePercent: 3m)
            ]));

        Assert.Equal(900m, result.TaxableGrossPay);
        Assert.Equal(90m, result.TaxLines.Single().Amount);
        Assert.Equal(710m, result.NetPay);
        Assert.Equal(200m, result.TotalEmployeeDeductions);
        Assert.Equal(30m, result.EmployerContributionTotal);
    }

    [Fact]
    public void AdjustmentCaps_RespectPerPeriodAndRemainingAnnualAmount()
    {
        var result = new TaxRuleEngine().Compute(new PayrollComputationRequest(
            BuildEmployee(), BuildProfile(), PayFrequency.BiWeekly,
            1000m, 80m, 0m, 0m, new DateOnly(2026, 4, 3), [],
            [new("CHARITY", "Charity", PayrollAdjustmentKind.PostTaxDeduction,
                FixedAmount: 75m, PerPeriodCap: 60m, AnnualCap: 500m, YearToDateAmount: 470m)]));

        Assert.Equal(30m, result.AdjustmentLines.Single().Amount);
        Assert.Equal(970m, result.NetPay);
    }

    private static Employee BuildEmployee()
        => new()
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = "T-01",
            FullName = "Test Employee",
            Department = "QA",
            WorkState = "MI",
            HourlyRate = 25m,
            Status = EmploymentStatus.Active
        };

    private static TaxProfile BuildProfile()
        => new()
        {
            EmployeeId = Guid.NewGuid(),
            FederalFilingStatus = FilingStatus.Single,
            StateCode = "MI",
            LocalTaxCode = "NONE"
        };
}
