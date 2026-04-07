FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoreBooking.API/CoreBooking.API.csproj CoreBooking.API/
COPY CoreBooking.Domain/CoreBooking.Domain.csproj CoreBooking.Domain/
COPY CoreBooking.Infrastructure/CoreBooking.Infrastructure.csproj CoreBooking.Infrastructure/
COPY Adapters.MeatSupplier/Adapters.MeatSupplier.csproj Adapters.MeatSupplier/
COPY Adapters.VeggieSupplier/Adapters.VeggieSupplier.csproj Adapters.VeggieSupplier/
COPY Adapters.SpiceSupplier/Adapters.SpiceSupplier.csproj Adapters.SpiceSupplier/
RUN dotnet restore CoreBooking.API/CoreBooking.API.csproj

COPY . .
WORKDIR /src/CoreBooking.API
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CoreBooking.API.dll"]
