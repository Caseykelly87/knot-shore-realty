# Knot Shore Realty

A small real estate website for a fictional St. Louis brokerage, built with ASP.NET Core 8 MVC. This is a portfolio project intended to demonstrate practical experience with C#, .NET, Entity Framework Core, REST API design, content modeling, testing, and CI/CD workflows.

![build](https://github.com/YOUR-USERNAME/knot-shore-realty/actions/workflows/build-and-test.yml/badge.svg)

#### Status ####

This project will be built in six phases. Each phase is a separate feature branch merged to main via pull request.

- [x] Phase 1 — Scaffold solution, EF Core, logging, health checks
- [x] Phase 2 — Domain models, repositories, seed data
- [ ] Phase 3 — Public pages, view components, SEO baseline
- [ ] Phase 4 — REST API with integration tests
- [ ] Phase 5 — Client-side filter and inquiry submission
- [ ] Phase 6 — ADRs, backlog, final documentation

## What this is

Knot Shore Realty is a fictional commercial and residential brokerage serving the St. Louis metro area. The site lets visitors browse active listings, filter by price and bedrooms, read about agents, explore neighborhoods, and submit inquiries about specific properties. It is deliberately scoped to the public-facing "marketing site" surface area — no agent login, no admin panel, no image upload, no payment processing.

The goal of the project is to demonstrate the skills a junior .NET developer is typically expected to bring on day one: building a structured multi-project solution, modeling content with Entity Framework Core, writing controllers and Razor views, exposing a REST API, writing meaningful unit and integration tests, and shipping via CI. Every technology choice is documented in the `docs/decisions/` folder as a short architecture decision record.

## Tech stack

- **.NET 8** and **ASP.NET Core MVC** for the web application
- **Entity Framework Core 8** with the **SQLite** provider for data access
- **xUnit** and **FluentAssertions** for unit and integration testing
- **Serilog** for structured logging to console and rolling files
- **Bootstrap 5** for layout and base styling (via CDN, no build step)
- **Vanilla JavaScript** with `fetch()` for the interactive listings filter
- **GitHub Actions** for continuous integration

Architecture decisions (why SQLite over Postgres, why View Components over partials, why a Repository + Service pattern, why vanilla JS over jQuery for the filter, why three projects instead of one) are documented in `docs/decisions/`.

## Solution layout

The solution is split into three projects plus a test project. This separation is deliberate and is explained in `docs/decisions/0001-project-structure.md`, but the short version is that it makes dependencies flow in one direction and keeps the domain logic free of framework concerns.


KnotShoreRealty.sln
|
+-- src/
|   +-- KnotShoreRealty.Core/        Domain models, interfaces, business services
|   +-- KnotShoreRealty.Data/        EF Core DbContext, repositories, migrations, seed data
|   +-- KnotShoreRealty.Web/         MVC controllers, Razor views, view components, API controllers
|
+-- tests/
|   +-- KnotShoreRealty.Web.Tests/   Unit tests and integration tests
|
+-- docs/
|   +-- decisions/                    Architecture decision records
|   +-- screenshots/                  README screenshots
|
+-- .github/
|   +-- workflows/
|       +-- build-and-test.yml        CI workflow
|
+-- BACKLOG.md                        Planned future features as user stories
+-- README.md                         This file

Dependencies flow in one direction: `Web` depends on `Core` and `Data`; `Data` depends on `Core`; `Core` depends on nothing else in the solution. This means the domain models and business logic in `Core` have no knowledge of EF Core, ASP.NET, or any external framework, which makes them trivial to unit test.

## Running locally

Prerequisites: .NET 8 SDK installed. No database server required — SQLite runs as a file in the project directory.

```bash
git clone https://github.com/YOUR-USERNAME/knot-shore-realty.git
cd knot-shore-realty
dotnet restore
dotnet build
dotnet ef database update --project src/KnotShoreRealty.Data --startup-project src/KnotShoreRealty.Web
dotnet run --project src/KnotShoreRealty.Web
```

The site will be available at `https://localhost:5001`. The database file `knotshorerealty.db` is created in the `src/KnotShoreRealty.Web/` directory and seeded with sample data on first run.

To run the tests:

```bash
dotnet test
```

## Features

### Public pages

| Page | Route | Description |
|---|---|---|
| Home | `/` | Hero section, three featured listings, three featured agents |
| Listings | `/listings` | Grid of all active and pending listings with a sidebar filter |
| Listing detail | `/listings/{id}` | Full property info, image gallery, assigned agent, inquiry form |
| Agents | `/agents` | Grid of all agents |
| Agent detail | `/agents/{id}` | Agent bio, contact info, list of their current listings |
| Neighborhoods | `/neighborhoods` | Taxonomy tree of regions, cities, and neighborhoods |
| Neighborhood detail | `/neighborhoods/{slug}` | Neighborhood description and filtered listings |
| About | `/about` | Static content about the brokerage |
| Inquiry confirmation | `/inquiries/submitted` | Shown after a successful inquiry POST |

### REST API

The API exists at `/api/` and returns JSON. It exists both to support the client-side listings filter and to demonstrate REST API design with proper status codes and model validation. All endpoints return standard HTTP status codes (200, 201, 400, 404).

| Method | Route | Description |
|---|---|---|
| GET | `/api/listings` | Filtered listings. Query params: `minPrice`, `maxPrice`, `minBeds`, `neighborhoodId`, `listingType` |
| GET | `/api/listings/{id}` | Single listing detail with nested agent and images |
| GET | `/api/neighborhoods` | Neighborhood taxonomy as a nested tree |
| POST | `/api/inquiries` | Submit an inquiry. Body: `{listingId, name, email, phone, message}` |

The API is versioned under `/api/` without a version segment for simplicity — this is a single-consumer API for the site's own frontend. In a real production system I would add `/api/v1/` and document why in an ADR.

### Health check

`GET /health` returns JSON indicating the status of the application and its database connection. This uses the built-in `Microsoft.Extensions.Diagnostics.HealthChecks` infrastructure and exists to demonstrate awareness of production-readiness patterns.

## Data model

Six entities, modeled in `KnotShoreRealty.Core/Models/`. Relationships and validation rules live on the domain models themselves; EF Core configuration lives in `KnotShoreRealty.Data/Configurations/` using `IEntityTypeConfiguration<T>`.

**Listing** — the core entity. Has an address, price, bedrooms, bathrooms, square footage, description, listing type (enum: `Residential` or `Commercial`), status (enum: `Draft`, `Active`, `Pending`, `Sold`, `Withdrawn`), listing date, and foreign keys to `Agent` and `Neighborhood`. Has a collection of `ListingImage` records. Only `Active` and `Pending` listings appear on public pages by default.

**Agent** — a real estate agent. Has name, title, bio, email, phone, photo URL, and a collection of assigned listings.

**Neighborhood** — part of a self-referential hierarchical taxonomy. Has a name, slug, description, hero image URL, and a nullable `ParentId` that points to another neighborhood. The hierarchy supports up to four levels with variable depth: metro region -> county or independent city -> city or town -> named neighborhood. Some branches go four levels deep, others stop earlier when the geography does not support deeper nesting. Listing detail pages render a breadcrumb from this hierarchy.

**ListingImage** — an image associated with a listing. Has a URL, alt text, and a sort order. One image per listing is marked as primary and used as the thumbnail.

**Inquiry** — a submission from the public inquiry form. Has a foreign key to the listing being inquired about, plus name, email, phone, message, and submitted-at timestamp. Has data annotation validation for email format, required fields, and message length.

**PropertyType** — a C# enum (`SingleFamily`, `Condo`, `Townhouse`, `MultiFamily`, `Office`, `Retail`, `Industrial`, `Land`) used as a property on `Listing`. Modeled as an enum rather than a lookup table because the values are stable and finite. This decision is documented in `docs/decisions/0004-enums-not-lookup-tables.md`.

### Listing status workflow

Listings move through a simple editorial workflow represented by the `ListingStatus` enum:

Draft -> Active -> Pending -> Sold -> Withdrawn

Only `Active` and `Pending` listings appear on the public listings page and in API responses by default. `Sold` listings remain visible on the assigned agent's detail page as a portfolio of completed deals. `Draft` and `Withdrawn` listings are hidden from all public views but remain in the database. The filtering rule is enforced in a single place — `ListingRepository.GetPublicListingsAsync()` — and covered by unit tests so that changes to the rule are caught immediately.

This workflow is deliberately simple but real: it demonstrates understanding of editorial states in a content-driven system without ballooning into a full role-based approval pipeline.

## Reusable UI components

The site uses **Razor View Components** rather than partial views for reusable UI pieces that have their own data-loading logic. View Components are ASP.NET Core's idiom for encapsulating a chunk of UI along with the code that populates it, and they are the closest MVC analog to a component-based rendering model.

Current View Components:

- `SiteNavigationViewComponent` — top navigation with the current page highlighted
- `FeaturedListingsViewComponent` — homepage featured listings block, pulls three random active listings
- `ListingCardViewComponent` — reusable listing card used on the home page, listings page, agent detail page, and neighborhood detail page
- `AgentCardViewComponent` — reusable agent card used on the home page and agents index
- `NeighborhoodBreadcrumbViewComponent` — breadcrumb trail rendered on listing detail pages from the neighborhood taxonomy

Partial views are used for pure markup that takes no parameters and has no logic (e.g., the site footer).

## Accessibility

The site is built to a baseline accessibility standard. This is not a formal WCAG audit, but it is a deliberate design priority:

- Semantic HTML: `<main>`, `<nav>`, `<article>`, `<aside>`, proper heading hierarchy with one `<h1>` per page
- All images have meaningful `alt` text sourced from the `ListingImage.AltText` field, not hardcoded
- All form inputs have associated `<label>` elements
- Color contrast verified against WCAG AA using browser dev tools
- Keyboard navigation works end-to-end: tab order is logical, focus states are visible, the listings filter is operable without a mouse
- The listings filter's JavaScript enhancement degrades to a standard GET form submission if JavaScript is disabled

## SEO

- Every page sets a meaningful `<title>` and `<meta name="description">` via a base layout and per-page `ViewData`
- Listing detail pages include Open Graph tags (`og:title`, `og:description`, `og:image`, `og:url`) for link previews
- `GET /sitemap.xml` generates a sitemap dynamically from the database, including the home page, listings index, all active and pending listings, all agent pages, and all neighborhood pages
- `/robots.txt` is served with a reference to the sitemap

The sitemap endpoint is particularly worth mentioning because it demonstrates content-driven URL generation — the URLs are derived from the database at request time, not hardcoded in a static file.

## Logging

Serilog is configured in `Program.cs` with two sinks: console output during development and a rolling daily file sink under `logs/` for longer-term review. Log levels and sink configuration live in `appsettings.json`.

Services use constructor-injected `ILogger<T>` and log structured events at meaningful points: inquiry submitted (with listing ID), listings filter applied (with parameters), validation failures, and unhandled exceptions via middleware. Logs use structured properties rather than string concatenation, so they are queryable in any log aggregation tool that understands Serilog's output format.

## Testing

Tests live in `tests/KnotShoreRealty.Web.Tests/` and use xUnit with FluentAssertions for readable assertions. The suite targets meaningful coverage of business logic rather than coverage percentage for its own sake. Specifically:

- **Listing search service** — the majority of tests. Covers empty filters, individual filter criteria, combined filters, edge cases (null, zero, negative values), and the status-filtering rule
- **Listing repository** — tested against the EF Core in-memory provider to verify query correctness without hitting SQLite
- **Inquiry validation** — data annotation rules for email format, required fields, and message length
- **Helper classes** — price formatter, neighborhood slug generator
- **API integration tests** — at least one test per API endpoint using `WebApplicationFactory<Program>`, asserting status codes and JSON shape

Razor views and controller pass-throughs are not unit-tested; their behavior is covered implicitly by the integration tests.

## Continuous integration

A GitHub Actions workflow at `.github/workflows/build-and-test.yml` runs on every push to any branch and every pull request to main. The workflow:

1. Checks out the code
2. Installs the .NET 8 SDK
3. Runs `dotnet restore`
4. Runs `dotnet build --configuration Release --no-restore`
5. Runs `dotnet test --configuration Release --no-build --verbosity normal`

The workflow uses an Ubuntu runner. The status badge at the top of this README reflects the current state of main.

## Development workflow

Work on this project follows a simple but deliberate branching model intended to mirror how a small team would actually ship code:

- `main` is always in a deployable state and reflects the most recent merged phase
- Each phase lives on its own feature branch (e.g., `feature/phase-2-data-layer`)
- Phases are merged to main via pull request, even though this is a solo project — the PR history itself is part of the portfolio
- Commit messages follow Conventional Commits (`feat:`, `fix:`, `test:`, `chore:`, `docs:`, `refactor:`)
- Small fixes and iterations are committed honestly rather than squashed into a single "perfect" commit per phase

## Architecture decision records

The `docs/decisions/` folder contains short markdown files documenting why specific technology choices were made. These exist because a portfolio project without context reads as "I picked whatever the tutorial used," and I wanted to show that each choice was deliberate.

- `0001-project-structure.md` — why three projects instead of one
- `0002-sqlite-over-postgres.md` — why SQLite is the right database for this specific project
- `0003-repository-and-service-pattern.md` — why I introduced this layer instead of controllers calling DbContext directly
- `0004-enums-vs-lookups.md` — why PropertyType is an enum and ListingStatus is an enum
- `0005-vanilla-js-over-jquery.md` — why the interactive filter uses vanilla `fetch()` rather than jQuery AJAX
- `0006-view-components-over-partials.md` — when I chose a View Component and when I chose a partial view

## Request lifecycle

One of the things I wanted to internalize while building this project was exactly what happens between a browser request and a rendered page. Here is the path a `GET /listings` request takes through the application:

1. The browser sends `GET /listings` to Kestrel
2. ASP.NET Core's routing middleware matches the URL against the `ListingsController.Index` action via conventional routing
3. Dependency injection resolves the controller's constructor dependencies: `IListingSearchService`, `INeighborhoodRepository`, `ILogger<ListingsController>`
4. The `Index` action calls `_listingSearchService.GetActiveListingsAsync()` with no filter parameters
5. The service calls `_listingRepository.GetPublicListingsAsync()`, which queries the `DbContext` for listings where `Status` is `Active` or `Pending`, including related `Agent`, `Neighborhood`, and primary `ListingImage` records via `.Include()`
6. EF Core translates the LINQ query to SQL, SQLite returns rows, EF Core materializes them as `Listing` entities
7. The service maps entities to a `ListingsIndexViewModel` and returns it to the controller
8. The controller calls `View(model)`, which locates `Views/Listings/Index.cshtml`
9. The Razor view renders the page, invoking the `ListingCardViewComponent` for each listing in the model
10. The rendered HTML is returned to the browser

The listings filter works the same way up through step 3, but then the browser's JavaScript takes over: it captures filter input changes, builds a `URLSearchParams` query string, calls `fetch('/api/listings?...')`, parses the JSON response, and re-renders the listing grid in place without a full page reload.

## Not in scope

This section exists because scope discipline is part of what the project is demonstrating. The following features are deliberately excluded and would be the first things I would add in a continuation of this project. They are listed in `BACKLOG.md` with acceptance criteria.

- User authentication and agent accounts
- Admin panel for managing listings
- Image upload (currently images are seeded as URLs)
- Real email sending for inquiries (currently only persisted to the database)
- Map view and geocoding
- Saved searches and favorites
- Pagination (the current seed data fits on one page)
- Full-text search
- Deployment to a cloud provider
- Internationalization
- Dark mode

## About Knot Shore Realty

Knot Shore Realty is a fictional brokerage created for this portfolio project. It is part of the Knot Shore family of fictional companies used across my portfolio projects for consistent branding — no real business exists under this name. All listings, agents, neighborhoods, and inquiries in the seeded data are fictional; addresses and descriptions are based on real St. Louis neighborhoods for realism but do not represent any actual property.

## License

MIT


