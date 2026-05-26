FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrbitShield.sln ./
COPY src/OrbitShield.Domain/OrbitShield.Domain.csproj src/OrbitShield.Domain/
COPY src/OrbitShield.Application/OrbitShield.Application.csproj src/OrbitShield.Application/
COPY src/OrbitShield.Infrastructure/OrbitShield.Infrastructure.csproj src/OrbitShield.Infrastructure/
COPY src/OrbitShield.Api/OrbitShield.Api.csproj src/OrbitShield.Api/

RUN dotnet restore OrbitShield.sln

COPY src/ src/
RUN dotnet publish src/OrbitShield.Api/OrbitShield.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrbitShield.Api.dll"]
