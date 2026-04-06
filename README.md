# Timekeeper

Timekeeper is a modern rebuild of the legacy `E:\TIMELOG` system in a new cross-platform .NET 10 solution rooted at `E:\Timekeeper`.

The new architecture keeps the original business shape of the old app:

- employees and payroll profiles
- time entry capture
- payroll preview and pay history
- tax rule configuration with effective dates and source tracking

It also borrows two important ideas from the companion systems you pointed me to:

- `E:\BrassLedger` for the modern .NET layering and packaging model
- `E:\TheSWShop\Zephyr` for data-driven tax calculation strategies instead of UI-bound special cases

## Current stack

- UI shell: Blazor Web App on .NET 10
- API: ASP.NET Core Web API
- Persistence: PostgreSQL via EF Core and Npgsql
- Desktop delivery: self-contained .NET publish with installer scripts for Windows, MSIX, macOS, Debian, and RPM

## Important tax note

The seeded tax rules in this first pass are intentionally marked `NeedsRefresh`.

That is by design. The old FoxPro tax defaults in `TIMELOG` and the calculation patterns in `Zephyr` are useful for migration and engine design, but they should not be treated as current tax law until replaced with validated current tables and rates.

## Quick start

Start PostgreSQL:

```powershell
docker compose up -d
```

Build:

```powershell
dotnet build .\Timekeeper.slnx
```

Run the web shell:

```powershell
dotnet run --project .\Timekeeper.Web
```

Run the API:

```powershell
dotnet run --project .\Timekeeper.Api
```

The default development connection string uses:

- host: `localhost`
- database: `timekeeper_dev`
- username: `postgres`
- password: `postgres`

## Packaging

Cross-platform publish helper:

```powershell
.\publish-timekeeper.ps1 -Runtime win-x64
```

MSIX package:

```powershell
.\packaging\windows\msix\package-msix.ps1 -PublishDir .\artifacts\win-x64 -Version 0.1.0 -OutputDir .\artifacts\installers
```

See [docs/publish-guide.md](docs/publish-guide.md) for all packaging commands.

## Repository layout

- `Timekeeper.Domain`: entities and enums
- `Timekeeper.Application`: queries, DTOs, and tax engine
- `Timekeeper.Infrastructure`: PostgreSQL, EF Core, seeding, and service implementations
- `Timekeeper.Api`: API endpoints
- `Timekeeper.Web`: desktop-delivered UI shell
- `Timekeeper.Tests`: unit tests
- `packaging`: installer and brand asset scripts
- `docs`: architecture and publishing notes
