# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/out

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
# The service listens on port 5000 by default
EXPOSE 5000
ENTRYPOINT ["dotnet", "FraudDetectorService.dll"]