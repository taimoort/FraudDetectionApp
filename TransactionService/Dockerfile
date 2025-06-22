# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build   
WORKDIR /src

# copy *only* the csproj files first for better layer-caching
COPY FraudDetectionApp.sln .
COPY TransactionService/*.csproj ./TransactionService/
COPY FraudDetectorService/*.csproj ./FraudDetectorService/
COPY Shared.Contracts/*.csproj    ./Shared.Contracts/
COPY Shared.Contracts/.config/dotnet-tools.json Shared.Contracts/.config/
# (add other project references here if needed)
RUN dotnet restore
RUN dotnet tool restore --tool-manifest Shared.Contracts/.config/dotnet-tools.json
# now copy the rest of the source
COPY . .

# publish self-contained binaries to /app/publish
RUN dotnet publish TransactionService \
    --configuration Release \
    --output /app/publish

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Kestrel will listen on port 80 inside the container
EXPOSE 80
ENTRYPOINT ["dotnet", "TransactionService.dll"]