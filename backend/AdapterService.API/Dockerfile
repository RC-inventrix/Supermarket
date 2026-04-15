# See https://aka.ms/customizecontainer to learn how to customize your debug container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# CHANGED: Removed the "backend/" prefixes so it builds correctly from your current directory
COPY ["AdapterService.API/AdapterService.API.csproj", "AdapterService.API/"]
COPY ["CoreBooking.Domain/CoreBooking.Domain.csproj", "CoreBooking.Domain/"]
RUN dotnet restore "./AdapterService.API/AdapterService.API.csproj"

COPY . .
WORKDIR "/src/AdapterService.API"
RUN dotnet build "./AdapterService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AdapterService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AdapterService.API.dll"]