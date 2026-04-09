FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the Core API and its two actual library dependencies
COPY ["CoreBooking.API/CoreBooking.API.csproj", "CoreBooking.API/"]
COPY ["CoreBooking.Domain/CoreBooking.Domain.csproj", "CoreBooking.Domain/"]
COPY ["CoreBooking.Infrastructure/CoreBooking.Infrastructure.csproj", "CoreBooking.Infrastructure/"]
RUN dotnet restore "./CoreBooking.API/CoreBooking.API.csproj"

# Copy the rest of the code and build
COPY . .
WORKDIR "/src/CoreBooking.API"
RUN dotnet build "./CoreBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoreBooking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoreBooking.API.dll"]