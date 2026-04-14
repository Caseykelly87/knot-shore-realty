# ADR 0007: Listing Model Represents the Listed Unit, Not Ownership or Tenure

**Date:** 2026-04-13
**Status:** Accepted

## Context

Early in the model design, a question arose about whether the `Listing` entity should carry fields that reflect CMS-style editorial concerns — featured flags, sort weights, rich content blocks — or remain a representation of the property being transacted. Similarly, it was unclear whether agent assignment should model a team, a primary contact, or ownership.

## Decision

The `Listing` entity models the unit of real estate being marketed, not the editorial or organizational context around it. Concretely:

- Agent assignment is a single FK (`AgentId`). There is no agent-team model, no co-listing concept, no featured-agent override.
- There is no `IsFeatured`, `SortWeight`, or similar editorial field on `Listing`. Presentation ordering is a view concern, not a domain concern.
- `ListingImage` models photos of the property. The `SortOrder` and `IsPrimary` fields exist to express display intent, but they are set by the seed process and are not an editorial CMS feature.
- `ListingStatus` tracks transaction lifecycle state (`Draft`, `Active`, `Pending`, `Sold`, `Withdrawn`), not publication state.

## Consequences

- The model stays narrow and reflects real estate domain concepts rather than CMS concepts. Controllers and views that need editorial behavior (e.g., pinning a listing to the top of a results page) must implement that logic themselves rather than reading a flag off the entity.
- Adding co-listing, team assignment, or featured flags in the future is a schema change. That cost is acceptable given that none of those requirements exist today.
- `ListingStatus.Draft` provides a lightweight publication gate without introducing a separate publication workflow.
