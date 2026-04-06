# Timekeeper Publish Guide

## Self-contained publish

Windows:

```powershell
dotnet publish .\Timekeeper.Web\Timekeeper.Web.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

macOS Intel:

```powershell
dotnet publish .\Timekeeper.Web\Timekeeper.Web.csproj `
  -c Release `
  -r osx-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

macOS Apple silicon:

```powershell
dotnet publish .\Timekeeper.Web\Timekeeper.Web.csproj `
  -c Release `
  -r osx-arm64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

Linux:

```powershell
dotnet publish .\Timekeeper.Web\Timekeeper.Web.csproj `
  -c Release `
  -r linux-x64 `
  --self-contained true `
  -p:PublishSingleFile=true
```

## Helper script

```powershell
.\publish-timekeeper.ps1 -Runtime win-x64
```

## Windows installers

Plain publish output:

- use `publish-timekeeper.ps1`

MSIX:

```powershell
.\packaging\windows\msix\package-msix.ps1 `
  -PublishDir .\artifacts\win-x64 `
  -Version 0.1.0 `
  -OutputDir .\artifacts\installers
```

Inno Setup:

- compile `packaging\windows\Timekeeper.iss` with Inno Setup after publishing

## macOS package

```bash
./packaging/macos/package-pkg.sh ./artifacts/osx-arm64 0.1.0 osx-arm64 ./artifacts/installers
```

## Linux packages

Debian:

```bash
./packaging/linux/package-deb.sh ./artifacts/linux-x64 0.1.0 ./artifacts/installers
```

RPM:

```bash
./packaging/linux/package-rpm.sh ./artifacts/linux-x64 0.1.0 ./artifacts/installers
```
