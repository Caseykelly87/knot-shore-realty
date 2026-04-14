# ADR 0003: Repository Pattern Over Direct DbContext

**Date:** 2026-04-13
**Status:** Accepted

## Context

Controllers and services need a way to query and persist domain data. The two common approaches are injecting `DbContext` directly or wrapping data access behind repository interfaces defined in Core.

## Decision

Define repository interfaces in `KnotShoreRealty.Core.Interfaces` and implement them in `KnotShoreRealty.Data.Repositories`. Controllers and services depend on the interfaces, not the concrete implementations.

## Consequences

- Controllers remain ignorant of EF Core; they interact only with interfaces from Core.
- Repository implementations can be tested in isolation using an in-memory SQLite database without spinning up the full web host.
- Adding a new query method requires a change to the interface, the implementation, and any tests — this is intentional, as it keeps the query surface explicit and discoverable.
- The pattern adds indirection that would be unnecessary for a script or one-off tool; the trade-off is justified here by the need for testable, layered architecture.
- A generic `IRepository<T>` base was considered and rejected: the repositories have meaningfully different query surfaces (e.g., `GetTaxonomyTreeAsync`, `GetAncestryAsync` on neighborhoods) that do not fit a uniform CRUD interface.
