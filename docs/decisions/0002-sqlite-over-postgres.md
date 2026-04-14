# ADR 0002: SQLite Over PostgreSQL

**Date:** 2026-04-13
**Status:** Accepted

## Context

The project is a portfolio application with a bounded, stable dataset (dozens of listings, a handful of agents, ~39 neighborhoods). It will be hosted on a single server or container. There is no requirement for concurrent writes from multiple application instances.

## Decision

Use SQLite as the primary database, accessed through EF Core's SQLite provider.

## Consequences

- No external database process is required in development or production; the database is a single file on disk.
- CI runs against the same SQLite provider used in production, so test and prod environments stay consistent.
- EF Core migrations work identically to any other relational provider, preserving the option to migrate to PostgreSQL later by swapping the provider and regenerating migrations.
- SQLite's decimal support requires storing `decimal` columns as `TEXT` (via `HasColumnType("TEXT")`); this is handled in the EF configurations and is a known limitation documented at the model level.
- Concurrent write scenarios (e.g., horizontal scaling) are not supported. Acceptable given the single-instance deployment target.
