# ADR 0001: Module-first modular monolith

- Status: Accepted
- Date: 2026-09-26

## Decision

The webshop is one ASP.NET Core process and deployment unit containing Catalog, Inventory, and Ordering bounded contexts. Each context owns Domain, Application, and Infrastructure projects, command and query EF Core contexts, a local unit of work, a PostgreSQL schema, and its migrations.

Cross-context calls are limited to explicit Application public contracts. Ordering reads Catalog sellability and current price synchronously. The reservation workflow crosses transaction boundaries through in-process integration-event contracts; event payloads contain UUIDs and values, never aggregates. Domain events remain private to their owning context and are published after successful EF persistence.

Each context retains the reference architecture's `DomainDbContext`/`QueryDbContext` split. PostgreSQL-specific context types remain internal to module infrastructure, and query models never create cross-context navigations or foreign keys.

HTTP read use cases use local `IQuery` ports implemented by EF adapters over the query contexts; writes use aggregate repositories and the command context's unit of work. Both sides read the same tables in the same physical database. The Catalog collaboration query evaluates its own aggregate's sellability rule and returns only its public DTO. All six contexts resolve the same `ConnectionStrings:Webshop` setting from DI at context creation, preserving host-level configuration overrides and module-specific migration histories.

## Boundary enforcement

Domain types (including aggregates, value objects, repositories, and domain events) and Application ports are internal. Domain grants friend access only to its own Application and Infrastructure assemblies and the test assemblies; Application grants access only to its own Infrastructure and unit tests. Main and other business modules do not receive friend access. Public Application types live exclusively in `Contracts.Public`; Infrastructure exposes only its module registration, endpoint mapping, and initialization entry point.

Architecture tests enforce these visibility rules, friend-assembly restrictions, inward layer dependencies, cross-module implementation isolation, and the independence of Shared support. EF model tests check all six command/query models for schema ownership and schema-local foreign keys. These checks protect code and mapped data boundaries; the shared development database credential does not enforce isolation against arbitrary raw SQL, which still requires review (or separate database roles in a hardened deployment).

The study questions map to these decisions: business capabilities define modules; visibility and tests enforce their boundaries; public contracts expose only collaboration data; Ordering queries Catalog synchronously before creating an order and publishes a committed fact for Inventory; schema-local persistence preserves each module's data ownership and Ordering's historical accepted price.

## Consequences

The codebase keeps strong module cohesion, compile-time dependency direction, one deployable, and independently owned relational schemas. It does not provide durable event delivery: a process failure after a local commit can leave an order Pending. Production hardening requires transactional outboxes, retries, idempotent consumers, monitoring, and reconciliation, optionally with a durable broker.
