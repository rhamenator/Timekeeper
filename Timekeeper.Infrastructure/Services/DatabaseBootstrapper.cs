using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Timekeeper.Domain.Entities;
using Timekeeper.Domain.Enums;
using Timekeeper.Infrastructure.Persistence;

namespace Timekeeper.Infrastructure.Services;

public sealed class DatabaseBootstrapper(TimekeeperDbContext dbContext, ILogger<DatabaseBootstrapper> logger)
{
    private static readonly Guid MayaId = Guid.Parse("25af473c-e208-4d22-8242-8d8d6398af16");
    private static readonly Guid JonahId = Guid.Parse("b0cc429b-9a0c-40cf-a7c7-a5b20d6fd65f");
    private static readonly Guid EliseId = Guid.Parse("4b29e3ec-61ca-4e93-b8cc-33fc0fc4f5fb");
    private static readonly Guid NoraId = Guid.Parse("1fc19c04-a96e-4cf5-b7f9-c8fddc654c30");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            await SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Timekeeper database bootstrap skipped. The app can build, but runtime data access will need a reachable PostgreSQL instance.");
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Employees.AnyAsync(cancellationToken))
        {
            return;
        }

        var employees = new[]
        {
            new Employee { Id = MayaId, EmployeeNumber = "OPS-001", FullName = "Maya Ortiz", Department = "Operations", WorkState = "MI", HourlyRate = 34.75m, PayFrequency = PayFrequency.BiWeekly },
            new Employee { Id = JonahId, EmployeeNumber = "ENG-014", FullName = "Jonah Briggs", Department = "Field Services", WorkState = "WA", HourlyRate = 42.50m, PayFrequency = PayFrequency.BiWeekly },
            new Employee { Id = EliseId, EmployeeNumber = "FIN-021", FullName = "Elise Harper", Department = "Finance", WorkState = "HI", HourlyRate = 39.25m, PayFrequency = PayFrequency.SemiMonthly },
            new Employee { Id = NoraId, EmployeeNumber = "ADM-042", FullName = "Nora Feld", Department = "Administration", WorkState = "NY", HourlyRate = 37.10m, PayFrequency = PayFrequency.Weekly }
        };

        var taxProfiles = new[]
        {
            new TaxProfile { EmployeeId = MayaId, FederalFilingStatus = FilingStatus.Single, FederalExemptions = 1, StateExemptions = 1, StateCode = "MI", LocalTaxCode = "LANSING" },
            new TaxProfile { EmployeeId = JonahId, FederalFilingStatus = FilingStatus.Married, FederalExemptions = 2, StateExemptions = 2, StateCode = "WA", LocalTaxCode = "SEATTLE" },
            new TaxProfile { EmployeeId = EliseId, FederalFilingStatus = FilingStatus.HeadOfHousehold, FederalExemptions = 1, StateExemptions = 1, StateCode = "HI", LocalTaxCode = "HONOLULU" },
            new TaxProfile { EmployeeId = NoraId, FederalFilingStatus = FilingStatus.Single, FederalExemptions = 0, StateExemptions = 0, StateCode = "NY", LocalTaxCode = "NYC" }
        };

        var payRunId = Guid.Parse("0b6fd70b-ac83-45d1-b215-3b0b286b1b1b");
        var payRun = new PayRun
        {
            Id = payRunId,
            Name = "2026-03 Closing Payroll",
            PeriodStart = new DateOnly(2026, 3, 1),
            PeriodEnd = new DateOnly(2026, 3, 15),
            CheckDate = new DateOnly(2026, 3, 20),
            Status = PayRunStatus.Posted
        };

        var payRunLines = new[]
        {
            new PayRunLine { Id = Guid.NewGuid(), PayRunId = payRunId, EmployeeId = MayaId, GrossPay = 2720.18m, FederalTax = 281.44m, StateTax = 125.13m, LocalTax = 5.71m, EmployerTax = 208.11m, NetPay = 2307.90m },
            new PayRunLine { Id = Guid.NewGuid(), PayRunId = payRunId, EmployeeId = JonahId, GrossPay = 3315.00m, FederalTax = 298.17m, StateTax = 0m, LocalTax = 0m, EmployerTax = 247.11m, NetPay = 3016.83m },
            new PayRunLine { Id = Guid.NewGuid(), PayRunId = payRunId, EmployeeId = EliseId, GrossPay = 3140.00m, FederalTax = 289.52m, StateTax = 14.11m, LocalTax = 0m, EmployerTax = 233.65m, NetPay = 2836.37m },
            new PayRunLine { Id = Guid.NewGuid(), PayRunId = payRunId, EmployeeId = NoraId, GrossPay = 1542.40m, FederalTax = 180.51m, StateTax = 61.70m, LocalTax = 7.71m, EmployerTax = 117.96m, NetPay = 1292.48m }
        };

        var timeEntries = new[]
        {
            NewEntry(MayaId, new DateOnly(2026, 3, 23), 8m, 1m, "BRIDGE", "Legacy time import candidate"),
            NewEntry(MayaId, new DateOnly(2026, 3, 24), 8m, 0m, "BRIDGE", "Crew scheduling support"),
            NewEntry(MayaId, new DateOnly(2026, 3, 25), 9m, 1m, "SHOP", "Back-office reconciliation"),
            NewEntry(JonahId, new DateOnly(2026, 3, 24), 10m, 2m, "FIELD", "Site diagnostics"),
            NewEntry(JonahId, new DateOnly(2026, 3, 25), 8m, 0m, "FIELD", "Equipment calibration"),
            NewEntry(EliseId, new DateOnly(2026, 3, 20), 8m, 0m, "LEDGER", "Month-end close"),
            NewEntry(EliseId, new DateOnly(2026, 3, 21), 7.5m, 0m, "LEDGER", "Tax notice review"),
            NewEntry(NoraId, new DateOnly(2026, 3, 27), 8m, 0m, "ADMIN", "Payroll approvals"),
            NewEntry(NoraId, new DateOnly(2026, 3, 28), 5m, 0m, "ADMIN", "Quarterly filing prep")
        };

        var rules = new[]
        {
            new TaxRule
            {
                Id = Guid.Parse("f3b3d0ff-e8cc-4bf7-84d6-a68d5d6dcbbb"),
                Code = "FED-LEGACY-ANNUAL",
                Name = "Federal withholding schedule (legacy starter)",
                JurisdictionKind = TaxJurisdictionKind.Federal,
                CalculationKind = TaxCalculationKind.AnnualizedBracket,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "*",
                RatePercent = 0m,
                EffectiveFrom = new DateOnly(1993, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new
                {
                    allowancePerExemption = 9.037m * 365m,
                    brackets = new[]
                    {
                        new { threshold = 0m, baseTax = 0m, ratePercent = 15m },
                        new { threshold = 3547m, baseTax = 532.05m, ratePercent = 28m },
                        new { threshold = 32923m, baseTax = 8753.33m, ratePercent = 31m }
                    }
                }),
                SourceSystem = "TIMELOG",
                SourceReference = "PAYCALCS.PRG + FEDTXSCH.DBF",
                Notes = "Seeded from the old FoxPro starter table. Treat as historical structure only, not current law."
            },
            new TaxRule
            {
                Id = Guid.Parse("3f1f7f9c-8c34-430b-8578-dd4cc8df0b40"),
                Code = "FICA-LEGACY-1993",
                Name = "FICA employee withholding",
                JurisdictionKind = TaxJurisdictionKind.Federal,
                CalculationKind = TaxCalculationKind.WageBasePercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "*",
                RatePercent = 6.2m,
                EffectiveFrom = new DateOnly(1993, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new { annualWageBase = 57600m }),
                SourceSystem = "TIMELOG",
                SourceReference = "txtable.dbf / PAYCALCS.PRG",
                Notes = "Legacy default imported from the historic timekeeping system."
            },
            new TaxRule
            {
                Id = Guid.Parse("882b9243-4108-4c99-b8d6-b11b81242824"),
                Code = "MED-LEGACY-1993",
                Name = "Medicare withholding",
                JurisdictionKind = TaxJurisdictionKind.Federal,
                CalculationKind = TaxCalculationKind.FlatPercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "*",
                RatePercent = 1.45m,
                EffectiveFrom = new DateOnly(1993, 1, 1),
                SourceSystem = "TIMELOG",
                SourceReference = "txtable.dbf / PAYCALCS.PRG",
                Notes = "Legacy default imported from the historic timekeeping system."
            },
            new TaxRule
            {
                Id = Guid.Parse("9c51c8d8-e960-435e-b4d0-7efcf7d0f27e"),
                Code = "MI-STATE-LEGACY",
                Name = "Michigan state withholding",
                JurisdictionKind = TaxJurisdictionKind.State,
                CalculationKind = TaxCalculationKind.FlatPercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "MI",
                RatePercent = 4.6m,
                EffectiveFrom = new DateOnly(1993, 1, 1),
                SourceSystem = "TIMELOG",
                SourceReference = "txtable.dbf / PAYCALCS.PRG",
                Notes = "Straight percentage rule preserved from TIMELOG for migration comparison."
            },
            new TaxRule
            {
                Id = Guid.Parse("0b22d658-86fb-4bc8-a293-95b84015ce1e"),
                Code = "LANSING-LOCAL-LEGACY",
                Name = "Local withholding with ceiling",
                JurisdictionKind = TaxJurisdictionKind.Local,
                CalculationKind = TaxCalculationKind.LocalCeilingPercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "LANSING",
                RatePercent = 0.21m,
                EffectiveFrom = new DateOnly(1993, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new { annualWageBase = 150000m }),
                SourceSystem = "TIMELOG",
                SourceReference = "txtable.dbf / PAYCALCS.PRG",
                Notes = "Modernized representation of the legacy local-percent-plus-ceiling pattern."
            },
            new TaxRule
            {
                Id = Guid.Parse("7672dd14-4cb0-4e59-9809-5f2c7be5cc4d"),
                Code = "WA-WC-ASSESS",
                Name = "Washington or Oregon hours-worked assessment",
                JurisdictionKind = TaxJurisdictionKind.Employer,
                CalculationKind = TaxCalculationKind.HoursWorked,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "WA",
                RatePercent = 0m,
                EffectiveFrom = new DateOnly(2000, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new { perHourAmount = 0.12m }),
                SourceSystem = "Zephyr",
                SourceReference = "taxengin.prg PTE_SDIF",
                Notes = "Captures the hours-based special-case pattern from the Zephyr payroll tax engine."
            },
            new TaxRule
            {
                Id = Guid.Parse("4b07d2be-45c0-434f-9a46-b13e0ac15f8e"),
                Code = "HI-SDIF-CAP",
                Name = "Hawaii SDI per-period cap",
                JurisdictionKind = TaxJurisdictionKind.State,
                CalculationKind = TaxCalculationKind.PerPeriodCapPercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "HI",
                RatePercent = 0.5m,
                EffectiveFrom = new DateOnly(2000, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new { perPeriodCap = 6.00m }),
                SourceSystem = "Zephyr",
                SourceReference = "taxengin.prg PTE_SDIF",
                Notes = "Represents the Hawaii pay-period-capped disability calculation pattern."
            },
            new TaxRule
            {
                Id = Guid.Parse("90cbce61-f541-4512-998c-495ff4b4170b"),
                Code = "NY-SDIF-CAP",
                Name = "New York SDI percent-cap variant",
                JurisdictionKind = TaxJurisdictionKind.State,
                CalculationKind = TaxCalculationKind.PerPeriodCapPercentage,
                Status = TaxRuleStatus.NeedsRefresh,
                RegionCode = "NY",
                RatePercent = 0.5m,
                EffectiveFrom = new DateOnly(2000, 1, 1),
                ParametersJson = JsonSerializer.Serialize(new { perPeriodCap = 8.00m }),
                SourceSystem = "Zephyr",
                SourceReference = "taxengin.prg PTE_SDIF",
                Notes = "Represents the New York special cap logic from the Zephyr engine."
            }
        };

        await dbContext.Employees.AddRangeAsync(employees, cancellationToken);
        await dbContext.TaxProfiles.AddRangeAsync(taxProfiles, cancellationToken);
        await dbContext.PayRuns.AddAsync(payRun, cancellationToken);
        await dbContext.PayRunLines.AddRangeAsync(payRunLines, cancellationToken);
        await dbContext.TimeEntries.AddRangeAsync(timeEntries, cancellationToken);
        await dbContext.TaxRules.AddRangeAsync(rules, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TimeEntry NewEntry(Guid employeeId, DateOnly workDate, decimal hours, decimal overtime, string projectCode, string notes)
        => new()
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            WorkDate = workDate,
            HoursWorked = hours,
            OvertimeHours = overtime,
            ProjectCode = projectCode,
            Notes = notes
        };
}
