# Creates the solution file and adds every project. Run once, from anywhere.
$ErrorActionPreference = "Stop"
Set-Location (Join-Path $PSScriptRoot "..")
if (Test-Path "HorizonLunaris.sln*") { Write-Host "Solution already exists."; exit 0 }
dotnet new sln -n HorizonLunaris
dotnet sln add `
  HorizonLunaris.Domain/HorizonLunaris.Domain.csproj `
  HorizonLunaris.Data/HorizonLunaris.Data.csproj `
  HorizonLunaris.Contracts/HorizonLunaris.Contracts.csproj `
  HorizonLunaris.Api/HorizonLunaris.Api.csproj `
  HorizonLunaris.Web/HorizonLunaris.Web.csproj `
  HorizonLunaris.Tests/HorizonLunaris.Tests.csproj
Write-Host "Done. Next: dotnet build"
