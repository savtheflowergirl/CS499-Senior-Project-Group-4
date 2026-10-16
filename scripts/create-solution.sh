#!/usr/bin/env bash
# Creates the solution file and adds every project. Run once, from anywhere.
set -euo pipefail
cd "$(dirname "$0")/.."
if compgen -G "HorizonLunaris.sln*" > /dev/null; then
  echo "Solution already exists."; exit 0
fi
dotnet new sln -n HorizonLunaris
dotnet sln add \
  HorizonLunaris.Domain/HorizonLunaris.Domain.csproj \
  HorizonLunaris.Data/HorizonLunaris.Data.csproj \
  HorizonLunaris.Contracts/HorizonLunaris.Contracts.csproj \
  HorizonLunaris.Api/HorizonLunaris.Api.csproj \
  HorizonLunaris.Web/HorizonLunaris.Web.csproj \
  HorizonLunaris.Tests/HorizonLunaris.Tests.csproj
echo "Done. Next: dotnet build"
