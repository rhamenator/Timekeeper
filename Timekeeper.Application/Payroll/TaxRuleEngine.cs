using System.Text.Json;
using Timekeeper.Domain.Entities;
using Timekeeper.Domain.Enums;

namespace Timekeeper.Application.Payroll;

public sealed class TaxRuleEngine
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public PayrollComputationResult Compute(PayrollComputationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.GrossPay < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Gross pay cannot be negative.");
        }
        if (request.DeferredAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Deferred amount cannot be negative.");
        }

        var adjustmentResults = new List<PayrollAdjustmentResult>();
        var remainingGross = request.GrossPay;
        var deferredAmount = Math.Min(request.DeferredAmount, remainingGross);
        if (deferredAmount > 0m)
        {
            adjustmentResults.Add(new PayrollAdjustmentResult(
                "DEFERRED",
                "Deferred compensation",
                PayrollAdjustmentKind.PreTaxDeduction,
                deferredAmount));
            remainingGross -= deferredAmount;
        }

        foreach (var adjustment in (request.Adjustments ?? [])
                     .Where(item => item.Kind == PayrollAdjustmentKind.PreTaxDeduction)
                     .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase))
        {
            var amount = Math.Min(ComputeAdjustmentAmount(adjustment, request.GrossPay), remainingGross);
            adjustmentResults.Add(ToResult(adjustment, amount));
            remainingGross -= amount;
        }

        var taxableGross = remainingGross;
        var lines = new List<TaxLineResult>();

        foreach (var rule in request.CandidateRules
                     .Where(rule => rule.Status != TaxRuleStatus.Retired)
                     .Where(rule => IsApplicable(rule, request))
                     .OrderBy(rule => rule.JurisdictionKind)
                     .ThenBy(rule => rule.Code))
        {
            var parameters = ParseParameters(rule.ParametersJson);
            var amount = rule.CalculationKind switch
            {
                TaxCalculationKind.FlatPercentage => taxableGross * (rule.RatePercent / 100m),
                TaxCalculationKind.WageBasePercentage => ComputeWageBaseTax(rule.RatePercent, taxableGross, request.YearToDateGrossPay, parameters.AnnualWageBase),
                TaxCalculationKind.HoursWorked => request.HoursWorked * parameters.PerHourAmount,
                TaxCalculationKind.PerPeriodCapPercentage => Math.Min(taxableGross * (rule.RatePercent / 100m), parameters.PerPeriodCap),
                TaxCalculationKind.LocalCeilingPercentage => ComputeLocalCeilingTax(rule.RatePercent, taxableGross, request.YearToDateGrossPay, parameters.AnnualWageBase),
                TaxCalculationKind.AnnualizedBracket => ComputeAnnualizedBracketTax(taxableGross, request.PayFrequency, request.TaxProfile.FederalExemptions, parameters),
                _ => 0m
            };

            amount = decimal.Round(Math.Max(0m, amount), 2, MidpointRounding.AwayFromZero);

            lines.Add(new TaxLineResult(
                rule.Code,
                rule.Name,
                rule.JurisdictionKind.ToString(),
                amount,
                rule.CalculationKind.ToString(),
                rule.Status.ToString()));
        }

        var totalTax = lines.Sum(line => line.Amount);
        var remainingNet = Math.Max(0m, request.GrossPay
            - adjustmentResults.Where(item => item.Kind == PayrollAdjustmentKind.PreTaxDeduction).Sum(item => item.Amount)
            - totalTax);

        foreach (var adjustment in (request.Adjustments ?? [])
                     .Where(item => item.Kind == PayrollAdjustmentKind.PostTaxDeduction)
                     .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase))
        {
            var amount = Math.Min(ComputeAdjustmentAmount(adjustment, request.GrossPay), remainingNet);
            adjustmentResults.Add(ToResult(adjustment, amount));
            remainingNet -= amount;
        }

        foreach (var adjustment in (request.Adjustments ?? [])
                     .Where(item => item.Kind == PayrollAdjustmentKind.EmployerContribution)
                     .OrderBy(item => item.Code, StringComparer.OrdinalIgnoreCase))
        {
            adjustmentResults.Add(ToResult(adjustment, ComputeAdjustmentAmount(adjustment, request.GrossPay)));
        }

        var employeeDeductions = adjustmentResults
            .Where(item => item.Kind != PayrollAdjustmentKind.EmployerContribution)
            .Sum(item => item.Amount);
        var employerContributions = adjustmentResults
            .Where(item => item.Kind == PayrollAdjustmentKind.EmployerContribution)
            .Sum(item => item.Amount);

        return new PayrollComputationResult(
            request.GrossPay,
            taxableGross,
            decimal.Round(remainingNet, 2, MidpointRounding.AwayFromZero),
            decimal.Round(employeeDeductions, 2, MidpointRounding.AwayFromZero),
            decimal.Round(employerContributions, 2, MidpointRounding.AwayFromZero),
            lines,
            adjustmentResults);
    }

    private static PayrollAdjustmentResult ToResult(PayrollAdjustment adjustment, decimal amount)
        => new(
            adjustment.Code,
            adjustment.Name,
            adjustment.Kind,
            decimal.Round(amount, 2, MidpointRounding.AwayFromZero));

    private static decimal ComputeAdjustmentAmount(PayrollAdjustment adjustment, decimal grossPay)
    {
        if (adjustment.FixedAmount < 0m || adjustment.RatePercent < 0m
            || adjustment.PerPeriodCap < 0m || adjustment.AnnualCap < 0m
            || adjustment.YearToDateAmount < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(adjustment), $"Adjustment {adjustment.Code} contains a negative amount.");
        }

        var amount = adjustment.FixedAmount + (grossPay * adjustment.RatePercent / 100m);
        if (adjustment.PerPeriodCap > 0m)
        {
            amount = Math.Min(amount, adjustment.PerPeriodCap);
        }
        if (adjustment.AnnualCap > 0m)
        {
            amount = Math.Min(amount, Math.Max(0m, adjustment.AnnualCap - adjustment.YearToDateAmount));
        }
        return decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    private static bool IsApplicable(TaxRule rule, PayrollComputationRequest request)
    {
        if (request.CheckDate < rule.EffectiveFrom)
        {
            return false;
        }

        if (rule.EffectiveTo is not null && request.CheckDate > rule.EffectiveTo.Value)
        {
            return false;
        }

        return rule.JurisdictionKind switch
        {
            TaxJurisdictionKind.Federal => !request.TaxProfile.FederalExempt,
            TaxJurisdictionKind.State => !request.TaxProfile.StateExempt && MatchesRegion(rule.RegionCode, request.TaxProfile.StateCode),
            TaxJurisdictionKind.Local => !request.TaxProfile.LocalExempt && MatchesRegion(rule.RegionCode, request.TaxProfile.LocalTaxCode),
            _ => MatchesRegion(rule.RegionCode, request.TaxProfile.StateCode) || rule.RegionCode == "*"
        };
    }

    private static bool MatchesRegion(string ruleRegion, string employeeRegion)
        => ruleRegion == "*" || string.Equals(ruleRegion, employeeRegion, StringComparison.OrdinalIgnoreCase);

    private static decimal ComputeWageBaseTax(decimal ratePercent, decimal taxableGross, decimal ytdGross, decimal wageBase)
    {
        if (wageBase <= 0m || ytdGross >= wageBase)
        {
            return 0m;
        }

        var taxableSlice = Math.Min(taxableGross, wageBase - ytdGross);
        return taxableSlice * (ratePercent / 100m);
    }

    private static decimal ComputeLocalCeilingTax(decimal ratePercent, decimal taxableGross, decimal ytdGross, decimal ceiling)
    {
        if (ceiling <= 0m)
        {
            return taxableGross * (ratePercent / 100m);
        }

        if (ytdGross >= ceiling)
        {
            return 0m;
        }

        var taxableSlice = Math.Min(taxableGross, ceiling - ytdGross);
        return taxableSlice * (ratePercent / 100m);
    }

    private static decimal ComputeAnnualizedBracketTax(decimal taxableGross, PayFrequency frequency, int exemptions, TaxRuleParameters parameters)
    {
        if (parameters.Brackets.Count == 0)
        {
            return 0m;
        }

        var annualFactor = frequency switch
        {
            PayFrequency.Daily => 365m,
            PayFrequency.Weekly => 52m,
            PayFrequency.BiWeekly => 26m,
            PayFrequency.SemiMonthly => 24m,
            PayFrequency.Monthly => 12m,
            PayFrequency.Quarterly => 4m,
            PayFrequency.SemiAnnual => 2m,
            _ => 1m
        };

        var annualizedGross = taxableGross * annualFactor;
        var adjustedAnnualizedGross = Math.Max(0m, annualizedGross - (parameters.AllowancePerExemption * exemptions));
        var bracket = parameters.Brackets
            .OrderBy(item => item.Threshold)
            .Last(item => adjustedAnnualizedGross >= item.Threshold);

        var annualTax = bracket.BaseTax + ((adjustedAnnualizedGross - bracket.Threshold) * (bracket.RatePercent / 100m));
        return annualTax / annualFactor;
    }

    private static TaxRuleParameters ParseParameters(string parametersJson)
        => string.IsNullOrWhiteSpace(parametersJson)
            ? new TaxRuleParameters()
            : JsonSerializer.Deserialize<TaxRuleParameters>(parametersJson, JsonOptions) ?? new TaxRuleParameters();
}

public sealed record TaxRuleParameters
{
    public decimal AnnualWageBase { get; init; }
    public decimal PerPeriodCap { get; init; }
    public decimal PerHourAmount { get; init; }
    public decimal AllowancePerExemption { get; init; }
    public IReadOnlyList<TaxBracket> Brackets { get; init; } = [];
}

public sealed record TaxBracket(decimal Threshold, decimal BaseTax, decimal RatePercent);
