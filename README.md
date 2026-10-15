# Vehicle Manager

A small ASP.NET Core (Razor Pages) web application for managing **vehicles** and **vehicle weight
categories**, built for the CreditWorks Software Engineer technical assignment. Data is stored in
Microsoft SQL Server via Entity Framework Core.

## Setup

### Required software

| Tool | Notes |
|------|-------|
| [.NET SDK 9](https://dotnet.microsoft.com/download) | To target another version, change `TargetFramework` in `Directory.Build.props` and the EF Core package versions (`9.0.0`) in the `.csproj` files |
| SQL Server 2019 or later | LocalDB (installed with Visual Studio), SQL Server Express, or Docker |
| `dotnet-ef` CLI tool | `dotnet tool install --global dotnet-ef` (only needed to run migrations manually) |

### Database

The default connection string in `src/VehicleManager.Web/appsettings.json` targets **SQL Server LocalDB**
using Windows authentication, so there are no credentials in source control:

```
Server=(localdb)\MSSQLLocalDB;Database=VehicleManager;Trusted_Connection=True;TrustServerCertificate=True
```

To use a different server (SQL Express, Docker, remote), override the connection string **without editing
tracked files** using user secrets or an environment variable:

```bash
# user secrets
dotnet user-secrets init --project src/VehicleManager.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=VehicleManager;User Id=sa;Password=<your-password>;TrustServerCertificate=True" --project src/VehicleManager.Web

# or environment variable (double underscore = section separator)
export ConnectionStrings__DefaultConnection="Server=...;Database=VehicleManager;..."
```

SQL Server in Docker, if you have no local instance:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<your-password>" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

### Create the database

Either of these creates the `VehicleManager` database, applies the schema migration and seeds the
manufacturers and initial categories:

1. **Automatic (Development):** just run the app (below). `appsettings.Development.json` sets
   `Database:MigrateOnStartup` to `true`, so pending migrations are applied at startup.
2. **Manual:**
   ```bash
   dotnet ef database update --project src/VehicleManager.Infrastructure --startup-project src/VehicleManager.Web
   ```

### Build, run, test

```bash
dotnet build
dotnet run --project src/VehicleManager.Web     # https://localhost:7080
dotnet test                                      # all tests
dotnet test --filter "Category=Unit"             # business-rule unit tests only (no database)
dotnet test --filter "Category=Integration"      # services + EF model against in-memory SQLite
```

(Bootstrap is loaded from a CDN for styling only; without internet the app works but looks unstyled.)

## Design notes

### Architecture

Three source projects plus tests, kept deliberately small:

```
src/VehicleManager.Core            Domain entities, business rules, services, repository interfaces. No dependencies.
src/VehicleManager.Infrastructure  EF Core DbContext, entity configuration, migrations, repository implementations.
src/VehicleManager.Web             Razor Pages UI. Thin: binds forms, calls services, maps errors to the page.
tests/VehicleManager.Tests         Unit tests (pure rules) and integration tests (services + real EF model).
```

Dependencies point inwards (`Web -> Infrastructure -> Core`, `Web -> Core`). Business rules live in
`Core/Rules` as pure static functions (no I/O), which makes them trivial to unit test. Services in
`Core/Services` orchestrate: load -> apply change to an in-memory copy -> validate the whole set -> persist.
Expected failures are returned as `Result` values (validation errors, not found) rather than thrown.

I did not add MediatR, CQRS, a generic repository or a separate Application project: for an application this
size those would be the "excessive layers" the brief warns against. Repository interfaces exist only where
they earn their keep (keeping `Core` free of EF Core).

### Database design

| Table | Columns / constraints |
|-------|----------------------|
| `Manufacturers` | `Id`, `Name` (unique, max 50). Seeded: Mazda, Mercedes, Honda, Ferrari, Toyota. Vehicles reference it by FK, so names are not embedded in code and new manufacturers are just rows. |
| `VehicleCategories` | `Id`, `Name` (unique), `IconKey`, `MinWeightKg decimal(10,2)` (unique, `>= 0`). Seeded: Light 0, Medium 500, Heavy 2500. |
| `Vehicles` | `Id`, `OwnerName`, `ManufacturerId` (FK, restrict delete), `YearOfManufacture`, `WeightKg decimal(10,2)` (`> 0`). |

Key decisions:

* **A vehicle's category is calculated, not stored.** There is no `CategoryId` on `Vehicles`, so the
  category can never be stale or inconsistent when definitions change (requirement 6), and deleting or
  editing a category never orphans vehicles. Cost: the list resolves categories at read time. With a handful of
  categories this is an in-memory lookup of a tiny cached-able table.
* **Only the lower bound of each range is stored.** A category's upper bound is the *next* category's lower
  bound. Gaps and overlaps are therefore not representable in the data, rather than merely validated against.
  The unique index on `MinWeightKg` is a database-level backstop against two categories starting at the same weight.
* **Icons are stored as a key** (e.g. `truck`) into a catalogue in `CategoryIcons`; the UI renders emoji.
  Changing the glyphs, or moving to SVG/image assets, needs no data migration.
* **Weight is `decimal(10,2)`** (exact, two decimal places, kilograms). Check constraints enforce `> 0`.

### Category calculation and boundary rules

* Ranges are **half-open: `[from, to)`** - the lower bound is inclusive, the upper bound exclusive.
* A weight belongs to the category with the greatest starting weight that is `<=` the weight.
* The initial data is therefore: **Light** `[0, 500)`, **Medium** `[500, 2500)`, **Heavy** `[2500, no limit)`.
  A vehicle weighing exactly `500.00 kg` is Medium; `499.99 kg` is Light; `2500.00 kg` is Heavy.
* Validation of a configuration (`CategoryConfigurationValidator`) requires: at least one category; the lowest
  category starts at 0 kg (**no gap**); no two categories share a starting weight (**no overlap**); unique names;
  a valid name and icon; starting weights are non-negative, have at most two decimals and are within the limit.
* Every change (create / edit / delete) is applied to an in-memory copy of the full configuration, validated
  **as a set**, and only then saved, inside a `Serializable` transaction.
* **Deleting** a category lets the category below absorb its range; deleting the lowest category extends the next
  one down to 0 kg; the last remaining category cannot be deleted.

### Validation and security

* All validation is **server-side** (shared by form posts and any future API); forms use `novalidate` so the
  server's messages are what users see. Database constraints (NOT NULL, lengths, unique indexes, check constraints)
  are a second line of defence.
* EF Core parameterises every query (no SQL built from user input). Razor encodes output. Anti-forgery tokens are
  added automatically to Razor Pages forms. Sort parameters from the query string are parsed against an enum.
* Unhandled exceptions render a generic error page with a reference id - no stack traces. Missing records give a 404 page.
* No secrets are committed: the default connection string uses Windows authentication to LocalDB;
  anything else is supplied via user secrets / environment variables.

### Assumptions

1. The brief's wording ("up to 500", "500 to 2500") is ambiguous at the boundaries; I chose lower-bound-inclusive
   half-open ranges (above) so each boundary weight maps to exactly one category.
2. Administrators define a category by its **starting weight**; the "to" value shown in the UI is derived.
3. The lowest category must start at 0 kg and the top category is unbounded, so every positive weight is covered.
4. Category names are unique (case-insensitive).
5. "Sensible" year of manufacture = 1886 (first practical motor vehicle) up to next calendar year.
6. Maximum weight is 1,000,000 kg (a sanity limit); weights must be > 0 with at most 2 decimal places.
7. Manufacturers are seeded reference data with no admin UI (the brief says they change rarely).
8. Vehicles can be added and listed, as required; editing/deleting vehicles was not requested.
9. The weight column is shown in the vehicle list (optional in the brief) because sorting by it is clearer that way.

### Known limitations

* No paging on the vehicle list (fine for a demo data size; see improvements).
* No authentication/authorisation - anyone can edit categories (explicitly out of scope; would be required in production).
* No optimistic-concurrency tokens on rows; concurrent category edits are serialised by the transaction instead.
* Icons are emoji, which render slightly differently per operating system.
* Integration tests use in-memory SQLite (fast, no setup) rather than SQL Server, so SQL Server-specific behaviour
  (e.g. collation, `decimal` ordering) is not exercised by tests. SQLite maps decimals to `double` in tests only.

### What I would improve for production

Server-side paging/filtering with indexes on the sort columns; authentication and an admin role for category
management; `rowversion` concurrency tokens and an audit trail for category changes; caching of the (tiny) category
table; integration tests against real SQL Server (Testcontainers) in CI; structured logging/metrics; a JSON API with
OpenAPI alongside the UI; an admin UI for manufacturers; and a CI pipeline building, testing and containerising the app.

### Automated tests

| Area | Where | What it protects |
|------|-------|------------------|
| Category determination & boundaries | `Unit/CategoryResolverTests` | 499.99 / 500 / 2500 boundaries, reconfigured boundaries (the 2200 kg example) |
| Gaps and overlaps | `Unit/CategoryConfigurationValidatorTests`, `Unit/CategoryBandsTests` | gap below lowest, duplicate starts (overlap), every weight in exactly one band |
| Vehicle validation | `Unit/VehicleValidatorTests` | required fields, year range, positive weight, max two decimals |
| Category administration | `Integration/CategoryServiceTests` | create/edit/delete against a real EF model; invalid changes are rejected and persist nothing; vehicles recategorise after changes |
| Vehicles & sorting | `Integration/VehicleServiceTests` | persistence, seeded manufacturers, every sort field in both directions |
