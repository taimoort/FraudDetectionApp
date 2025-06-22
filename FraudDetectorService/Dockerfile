# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1) Copy the solution and all .csproj references
COPY FraudDetectionApp.sln .
COPY Shared.Contracts/Shared.Contracts.csproj Shared.Contracts/
COPY TransactionService/TransactionService.csproj TransactionService/
COPY FraudDetectorService/FraudDetectorService.csproj FraudDetectorService/
COPY Shared.Contracts/.config/dotnet-tools.json Shared.Contracts/.config/

# 2) Restore *everything* (so the analyzers come down cleanly)
RUN dotnet restore
RUN dotnet tool restore --tool-manifest Shared.Contracts/.config/dotnet-tools.json
# 3) Copy the rest of your code into the image
COPY . .

# 4) Publish only the FraudDetectorService
WORKDIR /src/FraudDetectorService
RUN dotnet publish \
    --configuration Release \
    --output /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# 5) Copy the published bits into the runtime image
COPY --from=build /app/publish .

EXPOSE 5000
ENTRYPOINT ["dotnet", "FraudDetectorService.dll"]