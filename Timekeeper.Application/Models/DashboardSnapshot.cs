namespace Timekeeper.Application.Models;

public sealed record DashboardSnapshot(
    int ActiveEmployees,
    decimal CurrentPeriodHours,
    decimal GrossPreview,
    int RulesNeedingRefresh,
    IReadOnlyList<MetricCard> Metrics,
    IReadOnlyList<PayrollAlert> Alerts);

public sealed record MetricCard(string Label, string Value, string Accent);

public sealed record PayrollAlert(string Title, string Detail, string Severity);
