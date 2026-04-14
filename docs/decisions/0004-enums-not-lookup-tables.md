# ADR 0004: Closed Enums Over Lookup Tables

**Date:** 2026-04-13
**Status:** Accepted

## Context

Domain concepts like `ListingStatus`, `ListingType`, and `PropertyType` have a fixed, small set of values that are referenced throughout the application. Two approaches were considered: C# enums stored as strings, or dedicated lookup tables with foreign keys.

## Decision

Use C# enums (`ListingStatus`, `ListingType`, `PropertyType`) stored as strings in the database via EF Core's `HasConversion<string>()`. No lookup tables.

## Consequences

- Enum values are enforced at compile time; adding or removing a value requires a code change and migration, which is the correct level of friction for a closed set of domain concepts.
- String storage keeps the database human-readable without joining to a lookup table to decode integer codes.
- Filtering and querying against enum values is straightforward in EF Core — no joins required.
- If a value set needs to become user-configurable at runtime (e.g., allowing admins to add custom property types), this decision would need to be revisited and lookup tables introduced. That requirement does not exist for this project.
- Renaming an enum value is a breaking change to stored data; any rename must be accompanied by a migration that updates existing rows.
