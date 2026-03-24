# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY PawnShopV2.Domain/*.csproj PawnShopV2.Domain/
COPY PawnShopV2.Application/*.csproj PawnShopV2.Application/
COPY PawnShopV2.Infrastructure/*.csproj PawnShopV2.Infrastructure/
COPY PawnShopV2.API/*.csproj PawnShopV2.API/

RUN dotnet restore PawnShopV2.API/PawnShopV2.API.csproj

# Copy all source code and build
COPY . .
RUN dotnet publish PawnShopV2.API/PawnShopV2.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "PawnShopV2.API.dll"]
