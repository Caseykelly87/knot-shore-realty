# ADR 0001: Three-Project Clean Architecture Layout

**Date:** 2026-04-13
**Status:** Accepted

## Context

The project needed a solution structure that separates domain logic from persistence and presentation concerns, supports independent testing of each layer, and stays simple enough for a single developer to navigate without ceremony.

## Decision

Split the solution into three projects:

- **KnotShoreRealty.Core** — domain models, enums, interfaces, and helpers. No framework dependencies beyond the BCL.
- **KnotShoreRealty.Data** — EF Core DbContext, entity configurations, repository implementations, and seed infrastructure. References Core.
- **KnotShoreRealty.Web** — ASP.NET Core MVC host, controllers, views, and startup wiring. References both Core and Data.

A single test project (`KnotShoreRealty.Web.Tests`) covers all layers.

## Consequences

- Core can be tested in isolation without standing up a database or web host.
- Data implementations are swappable (e.g., switching from SQLite to PostgreSQL) without touching Core or Web.
- The single test project is simple to maintain at this scale; a per-layer split can be introduced if test counts or build times justify it.
- Web depends directly on Data for DI registration, which is an accepted trade-off — a pure dependency-inversion setup would require an additional infrastructure project or assembly scanning.
