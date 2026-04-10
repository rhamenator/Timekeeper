# Timekeeper Architecture

## Goals

Timekeeper modernizes the original FoxPro-era `TIMELOG` application while preserving the business value embedded in the old workflow and payroll/tax logic.

The design goals for this first milestone are:

- keep the domain small and understandable
- separate payroll/tax logic from screens
- make legacy tax provenance visible
- support cross-platform installers from day one

## Legacy sources

### TIMELOG

Primary contribution:

- employees
- pay history
- hours and period logic
- starter payroll defaults

Relevant legacy files included:

- `PAYCALCS.PRG`
- `PAYPROGS.PRG`
- `HOURS.PRG`
- `txtable.dbf`
- `fedtxsch.dbf`

### BrassLedger

Primary contribution:

- layered .NET solution structure
- desktop-delivered web shell pattern
- packaging conventions for Windows, macOS, and Linux

## Solution layers

### Domain

Contains entities and enums:

- `Employee`
- `TaxProfile`
- `TimeEntry`
- `PayRun`
- `PayRunLine`
- `TaxRule`

### Application

Contains:

- DTOs for the dashboard and UI
- `IWorkspaceQueryService`
- `TaxRuleEngine`

The engine supports multiple calculation styles so we can model legacy tax behavior without hard-coding each state into a screen:

- flat percentage
- wage-base percentage
- hours-worked assessments
- per-period capped percentage
- local ceiling percentage
- annualized bracket withholding

### Infrastructure

Contains:

- `TimekeeperDbContext`
- PostgreSQL configuration
- runtime database bootstrapper
- demo seed data
- query-service implementation

The seed data is deliberately annotated with `NeedsRefresh` to make stale tax content obvious.

### Web and API

Both the web shell and the API use the same infrastructure services.

That gives us:

- a desktop-style end-user app via the web shell
- a clean API surface for future integrations

## Tax rule posture

This first pass does not claim current tax accuracy. It claims accurate migration intent.

The schema is designed so that future work can:

- import current federal tables
- import state and local rate packs
- store effective-date history
- track provenance and validation status
- generate payroll calculations from auditable rule data
