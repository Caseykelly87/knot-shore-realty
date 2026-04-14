# 0007 — Content Management Patterns in a Small Real Estate Site

## Status

Accepted

## Context

Several modeling choices in this project are heavier than a 15-listing real estate site strictly requires. Specifically:

- Neighborhoods are modeled as a self-referential hierarchical taxonomy (metro → county → city → neighborhood) rather than a flat lookup table.
- Listings move through an editorial workflow represented by a `ListingStatus` enum (`Draft`, `Active`, `Pending`, `Sold`, `Withdrawn`) with rules about which states are visible on public pages.
- Reusable UI elements will be implemented in Phase 3 as Razor View Components rather than partial views, so each component owns both its markup and the logic that loads its data.

A simpler alternative exists for each of these. Neighborhoods could be a flat list with a single `City` foreign key. Listing visibility could be a boolean `IsActive` flag. Reusable UI could be plain partial views.

I chose the heavier patterns deliberately, and this ADR documents why.

## Decision

I am modeling this site using patterns common to content management systems, even where simpler alternatives would meet the immediate requirements of a portfolio project.

## Reasoning

Real estate is a content-driven domain. The interesting questions in a real estate site are not "how do we store a row" but "how do we model a hierarchy of locations, an editorial process around publishing a listing, and a library of reusable presentation components." Those are content-management questions, not CRUD questions. Modeling them at face value gives me practice with the patterns I would encounter in any production content-driven system.

The hierarchical neighborhood taxonomy reflects how real geographic data is actually structured. St. Louis is a metropolitan region that contains independent cities and counties, which contain cities and unincorporated areas, which contain named neighborhoods. The depth is uneven — some branches go four levels deep, some stop at two — and the data should reflect that. A flat lookup table would lose this structure entirely. A self-referential parent relationship preserves it and keeps the query model simple: every node knows its parent, every parent can find its children, and breadcrumbs are a walk up the chain.

The listing status workflow is a small but real editorial pipeline. A `Sold` listing is not a `Draft` listing is not an `Active` listing. They have different visibility rules, different lifecycles, and different audiences. Collapsing all of this into an `IsActive` boolean would lose information that matters in real estate operations — and more importantly, it would skip the exercise of modeling editorial state, which is a core concept in any system that publishes content.

View Components for reusable UI are the ASP.NET Core idiom for encapsulating a chunk of presentation along with the code that loads its data. A featured listings carousel, an agent card, a neighborhood breadcrumb — these are not just markup, they are markup plus a query. Partial views handle the markup half; View Components handle both. In a system where reusable components carry both responsibilities, View Components are the right tool, and they happen to be the closest analog in ASP.NET Core to the rendering composition model used by enterprise content management systems.

## Consequences

The codebase carries a small amount of ceremony beyond what the immediate feature set requires. The neighborhood query for the taxonomy tree loads all records and assembles the hierarchy in memory, which is slightly more code than a flat select. The listing visibility rule lives in a single repository method and is enforced by tests, which is more discipline than a boolean flag would need. View Components require their own classes and view files, which is a heavier touch than partial views.

In return, the project demonstrates awareness of patterns that show up in any content-driven system: hierarchical taxonomies, editorial workflows, and component-based rendering. These are the patterns I want to understand better, and the only way to become fluent is to use them on something real.

If the site ever grew to thousands of listings and dozens of taxonomy levels, the in-memory tree assembly would need to be replaced with a recursive CTE and the workflow might need to be promoted from an enum to a state machine with transition rules. Those are reasonable changes to make later when the data justifies them. They are not changes worth making preemptively at the current scale.

## Alternatives considered

**Flat neighborhood lookup with a `City` field.** Simpler, and adequate for a small site. Rejected because it loses the hierarchical structure that real geographic data has, and because flattening a tree to demonstrate flatness is the wrong direction of practice.

**`IsActive` boolean instead of `ListingStatus` enum.** Simpler, and adequate for a site that only needs to show or hide listings. Rejected because it collapses the distinction between editorial states (Draft, Pending, Sold) that have meaningfully different lifecycles and visibility rules in real estate.

**Partial views instead of View Components for reusable UI.** Simpler, and adequate for static markup. Rejected for components that need their own data loading; partial views can take a model parameter but cannot encapsulate the query that produces the model. View Components can do both.
