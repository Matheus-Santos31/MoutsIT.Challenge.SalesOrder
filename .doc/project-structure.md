[Back to README](../README.md)

## Project Structure

The solution follows **Clean/Hexagonal Architecture** (Ports & Adapters): `Domain` and `Application` are the core — they define interfaces (ports) and never depend on a concrete database, cache, message broker, or web framework. Everything under `src/` that talks to the outside world (Postgres, Redis, RabbitMQ, MongoDB, HTTP) is an adapter that implements one of those ports.

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/            # Core — entities, value objects, domain events, repository interfaces (ports)
│   ├── Ambev.DeveloperEvaluation.Application/        # Core — use cases (MediatR commands/handlers), validators, DTOs
│   ├── Ambev.DeveloperEvaluation.Common/              # Shared kernel — cross-cutting ports (IPasswordHasher, IMessagePublisher, ISalesReadModelStore) and utilities
│   │
│   ├── Ambev.DeveloperEvaluation.ORM/                 # Adapter (driven) — EF Core + PostgreSQL: repositories, migrations, Redis cache-aside
│   ├── Ambev.DeveloperEvaluation.MessageBus/          # Adapter (driven) — RabbitMQ publisher (+ logging fallback)
│   ├── Ambev.DeveloperEvaluation.ReadModel/           # Adapter (driven) — MongoDB sale-history read model
│   │
│   ├── Ambev.DeveloperEvaluation.WebApi/              # Adapter (driving) — REST API, controllers, JWT auth, Swagger
│   ├── Ambev.DeveloperEvaluation.OutboxProcessor/      # Adapter (driving) — background worker that dispatches the transactional outbox
│   │
│   └── Ambev.DeveloperEvaluation.IoC/                 # Composition root for the WebApi — wires ports to adapters at startup
│
├── tests/
│   ├── Ambev.DeveloperEvaluation.Unit/                # xUnit + NSubstitute + FluentAssertions + Bogus — handlers, domain rules, repository-level infra behavior
│   ├── Ambev.DeveloperEvaluation.Integration/          # Scaffolded, not yet populated
│   └── Ambev.DeveloperEvaluation.Functional/           # Scaffolded, not yet populated
│
├── docker-compose.yml           # Postgres, RabbitMQ, Redis, MongoDB, WebApi
├── run-local.bat                # One command: infra up + migrations + WebApi + OutboxProcessor
└── Ambev.DeveloperEvaluation.sln
```

### Why two separate driving adapters (WebApi and OutboxProcessor)

Both are entry points into the same core, but they serve different triggers: the WebApi reacts to HTTP requests, the OutboxProcessor reacts to a timer polling the `OutboxEvents` table. They're deployed and started independently (see [Getting Started](../README.md#getting-started)) and each has its own tiny composition root — the OutboxProcessor wires its handful of dependencies directly in its `Program.cs` rather than pulling in the WebApi's `IoC` project, since it doesn't need the other ~20 registrations that project holds.

### Why `ReadModel` and `MessageBus` are separate projects from `ORM`

Each is a distinct outbound integration with its own package dependencies (`MongoDB.Driver`, `RabbitMQ.Client`) and its own adapter implementing a port declared in `Common`. Keeping them isolated means the `Domain`/`Application` core, and even the `ORM` project itself, never need to reference Mongo or RabbitMQ client libraries — only whichever entry point (`WebApi`, `OutboxProcessor`) actually needs that integration references the adapter project that provides it.

<br>
<div style="display: flex; justify-content: space-between;">
  <a href="../README.md">Back to README</a>
</div>
