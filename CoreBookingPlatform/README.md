# Core Booking Platform

Core Booking Platform is a modular Food Market integration hub that centralizes catalog ingestion, availability checks, cart/order lifecycle, and checkout orchestration across multiple external suppliers.

## Architecture Overview

The platform is organized as a monorepo with a clear Core vs. Adapter separation:

- `backend/CoreBooking.API`: API entry point and orchestration layer.
- `backend/CoreBooking.Domain`: business entities and integration contracts.
- `backend/CoreBooking.Infrastructure`: persistence/infrastructure layer.
- `backend/Adapters.*`: pluggable supplier-specific integrations:
  - `Adapters.MeatSupplier`
  - `Adapters.VeggieSupplier`
  - `Adapters.SpiceSupplier`
- `frontend`: React + Vite client.

## Core vs. Adapters Design

The **Core** is supplier-agnostic and only depends on the `IExternalProviderAdapter` contract defined in `CoreBooking.Domain`.
Each adapter package implements this contract for one supplier and encapsulates provider-specific protocols, payload mapping, and checkout behavior.

This enables the platform to keep business logic stable while integrating new suppliers without changing core domain or API behavior.

## Open/Closed Principle via `IExternalProviderAdapter`

The system is **open for extension** and **closed for modification** by introducing new suppliers as new adapter projects that implement `IExternalProviderAdapter`.

Core services consume the interface, not concrete suppliers. As a result, onboarding a new provider is primarily an additive change:

1. Create a new adapter project.
2. Implement `IExternalProviderAdapter` methods for import, availability, and checkout.
3. Register the adapter in dependency injection.

No core domain contract changes are required for standard supplier onboarding.

## Running the Platform

From the monorepo root:

```bash
cd /home/runner/work/Supermarket/Supermarket/CoreBookingPlatform
docker-compose up --build
```

Services started by Docker Compose:

- SQL Server 2022 on `localhost:1433`
- API on `http://localhost:8080`
- Frontend on `http://localhost:3000`

## Monorepo Structure

```text
/CoreBookingPlatform
├── backend
│   ├── CoreBooking.sln
│   ├── CoreBooking.API
│   ├── CoreBooking.Domain
│   ├── CoreBooking.Infrastructure
│   ├── Adapters.MeatSupplier
│   ├── Adapters.VeggieSupplier
│   └── Adapters.SpiceSupplier
├── frontend
├── docker-compose.yml
├── .gitignore
└── README.md
```
