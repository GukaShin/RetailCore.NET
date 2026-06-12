# RetailCore.NET — Advanced POS and Inventory Management System

RetailCore.NET is a modern Point of Sale and inventory management system built with ASP.NET Core, Entity Framework Core, PostgreSQL, Redis, and Clean Architecture. It supports cashier checkout, barcode product search, multiple payment methods, shift management, refunds, inventory tracking, audit logs, and concurrency-safe stock updates.

> Status: Foundation vertical slice in progress. See the roadmap below.

## Why this project is non-trivial

The hardest backend challenge in a POS system is keeping sales and inventory correct under concurrency. If only one item is left in stock and two cashiers try to sell it at the same moment, exactly one sale must succeed and the other must fail with "Not enough stock" — stock must never go negative. RetailCore.NET solves this with database transactions, row-level locking, an optimistic concurrency token, and idempotency keys to prevent duplicate sales from retried requests.

## Tech stack

- ASP.NET Core Web API (.NET 9)
- Entity Framework Core 9 + PostgreSQL (Npgsql)
- Redis (caching, counters, idempotency)
- MediatR (CQRS) + FluentValidation
- JWT authentication + refresh tokens, role-based authorization
- Serilog structured logging
- xUnit + Testcontainers for unit, integration, and concurrency tests

## Architecture (Clean Architecture)

```
src/
  RetailCore.Domain          Core entities, enums, value objects, domain events
  RetailCore.Application      CQRS commands/queries, services, validators, interfaces, DTOs
  RetailCore.Contracts        Shared request/response and integration event models
  RetailCore.Infrastructure   EF Core DbContext, repositories, Redis, JWT, hashing
  RetailCore.Api              Controllers, middleware, auth wiring, exception handling
tests/
  RetailCore.Tests            Unit + integration + concurrency tests
```

## Getting started

Prerequisites: .NET 9 SDK, Docker (for PostgreSQL + Redis and Testcontainers).

```bash
# start backing services
docker compose up -d

# run the API
dotnet run --project src/RetailCore.Api

# run the tests
dotnet test
```

## Roadmap

- [x] Phase 1: Solution structure, projects, packages
- [ ] Phase 2: Domain entities + EF Core + migrations + seed
- [ ] Phase 3: Authentication and authorization
- [ ] Phase 4: Products, categories, inventory
- [ ] Phase 5: Shift management
- [ ] Phase 6-7: Cart calculation + concurrency-safe checkout
- [ ] Later: payments depth, refunds, discount engine, SignalR, RabbitMQ + Worker, Blazor frontends, .NET Aspire, Docker, stress tests
