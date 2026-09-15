# ApiVersioningDemo

Companion sample for a technical article on **API Versioning in ASP.NET Core** using the modern `Asp.Versioning` packages on **.NET 10**.

The project shows how to evolve an HTTP contract (V1 → V2) without duplicating business logic, and how to document, deprecate, and sunset versions cleanly.

---

## 1. What API Versioning is

API versioning lets you publish **multiple compatible contracts** for the same resource at the same time. Clients pin to a version (`v1`, `v2`, …). You can introduce breaking changes in a new version while older clients keep working until they migrate.

---

## 2. Why breaking changes require versioning

Without versioning, renaming `name` → `displayName` or nesting `price` into `pricing` breaks every existing client on the next deploy.

This sample’s Product API demonstrates that break on purpose:

| Version | Shape |
|--------|--------|
| **V1** | Flat `{ id, name, price }` |
| **V2** | `{ id, displayName, pricing: { amount, currency }, description }` |

Same domain model and application service; different DTOs.

---

## 3. Project architecture

```text
Domain Model
     ↓
Application Service
     ↓
 ┌───────────────┐
 │               │
API V1         API V2
 │               │
DTO V1         DTO V2
```

| Project | Role |
|---------|------|
| `ApiVersioningDemo.Domain` | `Product`, in-memory repository |
| `ApiVersioningDemo.Application` | Shared `IProductService` / `ProductService` |
| `ApiVersioningDemo.Api` | Versioned controllers, Minimal APIs, OpenAPI/Swagger UI |
| `ApiVersioningDemo.IntegrationTests` | Contract + header + unsupported-version tests |

**Principle:** version the **API contract**, not the domain/business logic (unless behavior itself changed).

---

## 4. Packages used

| Package | Why |
|---------|-----|
| `Asp.Versioning.Mvc` | Controller versioning (+ pulls in `Asp.Versioning.Http` for Minimal APIs) |
| `Asp.Versioning.Mvc.ApiExplorer` | Version-aware API explorer / OpenAPI metadata |
| `Asp.Versioning.OpenApi` | `AddOpenApi()` + `WithDocumentPerVersion()` for .NET 10 OpenAPI |
| `Microsoft.AspNetCore.OpenApi` | Built-in OpenAPI document generation |
| `Swashbuckle.AspNetCore.SwaggerUI` | Interactive Swagger UI over the built-in OpenAPI documents |

**Not used:** obsolete `Microsoft.AspNetCore.Mvc.Versioning`, EF Core, Scalar. OpenAPI documents still come from `Microsoft.AspNetCore.OpenApi` + `Asp.Versioning.OpenApi`; Swagger UI only renders them.

`Asp.Versioning.Http` is **not** added separately — it arrives transitively via `Asp.Versioning.Mvc`.

---

## 5. How URL Segment Versioning works

Primary reader:

```csharp
options.ApiVersionReader = new UrlSegmentApiVersionReader();
```

Routes use:

```text
api/v{version:apiVersion}/products
```

Examples:

```text
GET /api/v1/products
GET /api/v1/products/{id}
GET /api/v2/products
GET /api/v2/products/{id}
```

Because the version is required in the path, this sample does **not** enable `AssumeDefaultVersionWhenUnspecified` for normal Product traffic.

---

## 6. V1 vs V2 Product contract

**V1**

```json
{
  "id": 1,
  "name": "Laptop",
  "price": 25000
}
```

**V2**

```json
{
  "id": 1,
  "displayName": "Laptop",
  "pricing": {
    "amount": 25000,
    "currency": "EGP"
  },
  "description": "14-inch business laptop"
}
```

Mapping lives in `Mappings/ProductMappings.cs` (`ToV1Response` / `ToV2Response`).

---

## 7. Separate controllers per version

```text
Controllers/V1/ProductsController.cs   [ApiVersion(1.0, Deprecated = true)]
Controllers/V2/ProductsController.cs   [ApiVersion(2.0)]
```

Both inject `IProductService`. Only mapping differs.

---

## 8. MapToApiVersion example

`SystemInfoController` advertises both versions and maps actions with `[MapToApiVersion(1.0)]` / `[MapToApiVersion(2.0)]`:

```text
GET /api/v1/system/info
GET /api/v2/system/info
```

Useful when only a few endpoints diverge; for large contract breaks, prefer separate controllers (Products).

---

## 9. Convention-based versioning

`ConventionVersioningController` has **no** `[ApiVersion]` attributes. Versions are applied in `Program.cs`:

```csharp
options.Conventions
    .Controller<ConventionVersioningController>()
    .HasApiVersion(1.0)
    .HasApiVersion(2.0);
```

Useful when metadata should be centralized, controllers live in another assembly, or attributes are undesirable.

```text
GET /api/v1/convention-demo
GET /api/v2/convention-demo
```

---

## 10. Minimal API versioning

Orders are versioned separately from Products:

```text
GET /api/v1/orders
GET /api/v2/orders
```

Uses `NewApiVersionSet`, `MapGroup("api/v{version:apiVersion}/orders")`, `WithApiVersionSet`, and `MapToApiVersion`. V2 renames fields and nests cost — another intentional breaking change.

---

## 11–13. Query string, header, and media type examples

See `Examples/VersioningStrategies.cs` (compiles, **not** registered at runtime so it does not conflict with URL segment versioning):

| Strategy | Example |
|----------|---------|
| Query string | `GET /api/products?api-version=1.0` |
| Header | `GET /api/products` + `X-Api-Version: 2.0` |
| Media type | `Accept: application/json;v=2.0` (valid but less common for typical business APIs) |

---

## 14. Combining version readers

Also in `VersioningStrategies.cs`:

```csharp
ApiVersionReader.Combine(
    new QueryStringApiVersionReader("api-version"),
    new HeaderApiVersionReader("X-Api-Version"));
```

Helpful during migrations; avoid permanent ambiguity without a clear requirement.

---

## 15. Deprecation

V1 Product (and System Info V1) use:

```csharp
[ApiVersion(1.0, Deprecated = true)]
```

With `ReportApiVersions = true`, responses include:

```http
api-supported-versions: 2.0
api-deprecated-versions: 1.0
```

Active versions appear in `api-supported-versions`. Deprecated versions are listed separately in `api-deprecated-versions` (they are still callable). Asp.Versioning 10 may also emit RFC 8594 `Sunset` / `Link` and a `Deprecation` header when policies are configured.

**Deprecated means still available** — clients should migrate, not that the route is dead.

---

## 16. Sunset lifecycle

Demo policy dates (labelled as DEMO in code):

| Stage | DEMO date |
|-------|-----------|
| V1 deprecated | 2026-12-01 |
| V1 sunset | 2027-06-01 |

Configured via `options.Policies.Deprecate(1.0)` and `options.Policies.Sunset(1.0)` (RFC 8594 `Sunset` / related `Link` headers).

Lifecycle taught by the sample:

```text
Active → Deprecated → Migration Period → Sunset → Removed
```

---

## 17. OpenAPI / Swagger UI

- Documents: `/openapi/v1.json`, `/openapi/v2.json`
- UI: `/swagger` (definition dropdown switches between `v1` and `v2`)
- `SubstituteApiVersionInUrl = true` so docs show `/api/v1/products`, not `/api/v{version}/products`

---

## 18. Running the project

```bash
dotnet restore
dotnet build
dotnet run --project src/ApiVersioningDemo.Api --launch-profile https
```

- API: `https://localhost:7211`
- Swagger UI: `https://localhost:7211/swagger`
- OpenAPI V1: `https://localhost:7211/openapi/v1.json`
- OpenAPI V2: `https://localhost:7211/openapi/v2.json`

HTTP-only: `http://localhost:5149` (profile `http`).

---

## 19. Running tests

```bash
dotnet test
```

---

## 20. Example curl commands

```bash
# V1 product (deprecated contract)
curl -i https://localhost:7211/api/v1/products/1

# V2 product (nested pricing)
curl -i https://localhost:7211/api/v2/products/1

# V1 list
curl https://localhost:7211/api/v1/products

# Minimal APIs
curl https://localhost:7211/api/v1/orders
curl https://localhost:7211/api/v2/orders

# Unsupported version
curl -i https://localhost:7211/api/v99/products

# OpenAPI documents
curl https://localhost:7211/openapi/v1.json
curl https://localhost:7211/openapi/v2.json
```

On Windows PowerShell you may need:

```powershell
curl.exe -i https://localhost:7211/api/v1/products/1 -k
```

---

## 21. Recommended production practices

1. Prefer **URL segment** versioning for public REST APIs.
2. Version **contracts** (DTOs), share application/domain logic.
3. Ship with versioning from day one (`v1`), even if you only have one version.
4. Set `ReportApiVersions = true`.
5. Deprecate with `[ApiVersion(..., Deprecated = true)]`, announce a **Sunset** date, then remove — never silent deletes.
6. Keep **one OpenAPI document per version**; do not merge V1/V2 into one confusing doc.
7. Use `ApiVersionReader.Combine` only during migrations.
8. Do not enable `AssumeDefaultVersionWhenUnspecified` unless you are protecting legacy unversioned callers.
9. Avoid obsolete `Microsoft.AspNetCore.Mvc.Versioning` packages; use `Asp.Versioning.*` v10+ on .NET 10.
10. Treat major contract breaks as new versions; additive optional fields usually do not need a new version.

---

## Solution layout

```text
ApiVersioningDemo.sln
src/
  ApiVersioningDemo.Api/
  ApiVersioningDemo.Application/
  ApiVersioningDemo.Domain/
tests/
  ApiVersioningDemo.IntegrationTests/
README.md
```
