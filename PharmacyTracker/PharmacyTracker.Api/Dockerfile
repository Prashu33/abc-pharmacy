# Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PharmacyTracker.Api/PharmacyTracker.Api.csproj", "PharmacyTracker.Api/"]
RUN dotnet restore "PharmacyTracker.Api/PharmacyTracker.Api.csproj"
COPY . .
WORKDIR "/src/PharmacyTracker.Api"
RUN dotnet build "PharmacyTracker.Api.csproj" -c Release -o /app/build

# Publish release
FROM build AS publish
RUN dotnet publish "PharmacyTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Ensure Data directory exists inside container
RUN mkdir -p Data
ENTRYPOINT ["dotnet", "PharmacyTracker.Api.dll"]
