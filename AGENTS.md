# AGENTS.md

Guidance for AI coding agents working in the **centraLM** repository.

## Prime Directive

**Write no comments in code.** Not in C#, not in TypeScript/Svelte, not in config or scripts. This overrides convenience and habit.

- No `//`, `/* */`, XML doc comments (`///`), or TSDoc/JSDoc blocks.
- No section banners, numbered step markers, or "explains the next line" narration.
- No commented-out code — delete it; git remembers.
- Leave existing comments alone unless already editing that code; do not add new ones.
- Unsure whether something is a comment? It is. Skip it.

**The code must be clean and describe itself.** When a construct feels like it needs a comment, fix the code instead:

| Smell | Fix |
| --- | --- |
| Unclear intent | Rename the type, method, or variable to say what it means |
| Long method doing several things | Extract well-named private methods |
| Magic value | Named constant or a well-named field/enum |
| Non-obvious conditional | Extract a predicate like `IsOnboardingComplete(...)` |
| Complex expression | Extract a named local |
| Tricky invariant | Encode it in the type, or enforce it in a validator/guard clause |

Only allowed exceptions, and only when genuinely warranted: a licensing/attribution header required by a third-party license, or a directive pragma such as `#nullable enable`, `// @ts-expect-error`, or a linter-disable pragma. These are tooling directives, not prose — keep them to a single line and never add explanation around them.

## Project Overview

centraLM is a full-stack application split into two halves:

- **Backend** — ASP.NET Core Web API (.NET 10) following Clean Architecture, exposing minimal API endpoints and using a CQRS-style mediator pipeline.
- **Frontend** — SvelteKit (Svelte 5) single-page app using DaisyUI/Tailwind CSS and OpenID Connect for authentication.

Authentication/SSO is provided by **Authentik** via OIDC/JWT. Users are keyed in the database by their OpenID `issuer` + `subject`.

> **Stack commitments — do not substitute:**
> - Use **Mediator.Abstractions / Mediator.SourceGenerator** (`Mediator` namespace), **NOT MediatR**. The API is `IMediator.Send(...)`.
> - Use **FluentValidation** for request validation and **FluentResults** (`Result<T>`) for handler return values.
> - Use **Mapster** (`.Adapt<T>()`) for mapping between DTOs, commands, and domain models.
> - Data layer is **EF Core**. Postgres is the intended production provider; the current `Infrastructure/DependencyInjection.cs` still registers `UseInMemoryDatabase("InMemoryDatabase")` and there is no `Npgsql.EntityFrameworkCore.PostgreSQL` package yet. Treat Postgres support as in-progress and keep provider registration isolated in `AddInfrastructure`.
> - Frontend UI is **DaisyUI** on **Tailwind CSS v4** — prefer DaisyUI component classes (`btn`, `input`, `card`, `textarea`, `prose`, …) over custom CSS.

## Repository Layout

```
centraLM.sln
Domain/            # Entities, enums, no external dependencies
Infrastructure/    # EF Core DbContext + DI; references Domain
Application/       # Use cases (commands/queries), validators, services, mapping; references Infrastructure
API/               # ASP.NET host, minimal API endpoints, DTOs, auth; references Application
Frontend/          # SvelteKit app
```

**Dependency direction:** `API → Application → Infrastructure → Domain`. Domain must not reference other projects. Infrastructure must not reference Application. Keep business logic in Application, not in endpoints.

### Backend folders (follow the existing convention)

- `Application/<Feature>/Commands/` — command/query records + handlers (e.g. `Groups/Commands/CreateGroup.cs`).
- `Application/<Feature>/Validators/` — validators when kept in a separate file (some validators live beside the command).
- `Application/Behaviors/` and `Application/Pipelines/` — Mediator pipeline behaviors.
- `Application/Services/` — reusable domain services (`GroupAccessService`, `UserService`).
- `API/Endpoints/` — static classes with `Add<X>Endpoints(this WebApplication app)`.
- `API/DTO/` — request/response DTOs.
- `API/EndpointFilters/`, `API/Extensions/`, `API/Models/` — filter/binding helpers.

## Backend Conventions

### Commands & queries (Mediator + FluentResults)

A feature is a record implementing `ICommand<Result<T>>` / `IQuery<Result<T>>` plus a handler. Validators are registered from the Application assembly automatically. Example pattern (`Application/Groups/Commands/CreateGroup.cs`):

```csharp
public sealed record CreateGroupCommand(string Name, string Description, string? ParentGroupId, string UserId)
    : ICommand<Result<Group>>;

public sealed class CreateGroupCommandHandler(Context context, GroupAccessService groupAccessService)
    : ICommandHandler<CreateGroupCommand, Result<Group>>
{
    public async ValueTask<Result<Group>> Handle(CreateGroupCommand command, CancellationToken cancellationToken)
    {
        var entity = new Group { /* ... */ };
        await context.SaveChangesAsync(cancellationToken);
        return Result.Ok(entity);
    }
}
```

Conventions to preserve:
- `sealed record` requests and `sealed class` handlers; **primary constructors** for dependency injection.
- Handlers return `Result` / `Result<T>`; signal failure with `Result.Fail(...)` / `Result.Fail<T>(errors)` and success with `Result.Ok(...)`. Do not throw for expected business failures.
- `async ValueTask<T>` handler signatures, always thread the `CancellationToken`.
- EF Core queries use `AsNoTracking()` for reads.
- The optional `IResultQuery<T>` / `IResultCommand<T>` marker interfaces in `Application/Interfaces/IResultQuery.cs` exist for the validation pipeline; not all handlers implement them.

### Validation

- Validators are `AbstractValidator<TRequest>`; `AddValidatorsFromAssembly` registers them.
- Two behaviors exist and both are wired:
  - `Application/Behaviors/ValidationBehavior.cs` — for `IBaseCommand` requests; throws `ValidationException` on failure (registered in `API/Program.cs`).
  - `Application/Pipelines/ValidationPipeline.cs` — for `IResultQuery<T>`; returns `Result.Fail(...)`.
- Keep validators close to their request type. Watch for duplicate validators for the same request (e.g. `CreateGroupCommandValidator` and `Groups/Validators/CreateGroupValidator.cs` both exist) — prefer consolidating rather than adding a third.

### Endpoints (Minimal APIs)

- Endpoints are registered explicitly in `API/Program.cs` (`app.AddGroupEndpoints(); app.AddUserEndpoints();`). Add new `Add<X>Endpoints` calls there.
- Groups use `.RequireAuthorization()` and `.RequireUser()` (an endpoint filter that resolves the app user via `AuthUser.BindAsync`). The `AuthUser` record binds `UserService.GetUserId(context.User)` into a handler parameter.
- `ClaimsPrincipal` is injected directly when raw claims are needed; `Application.Extensions.GetSubIss()` / `API.Extensions.GetId()` extract the OpenID subject/issuer.
- Translate results to HTTP explicitly:
  ```csharp
  return result.IsFailed
      ? Results.BadRequest(result.Errors)
      : Results.Ok(result.Value.Adapt<GroupListingDto>());
  ```
  Add `.Produces<TDto>()` for OpenAPI.
- Request bodies use `[FromBody]` DTOs, then `dto.Adapt<Command>() with { ... }`.

### Mapping

Mapster is used everywhere; prefer `.Adapt<T>()` over hand-written mapping. DTOs may derive from `Domain.Models.Base` (`API/DTO/GroupDto.cs`).

### Domain models

- All entities derive from `Base` (string `Id` defaulting to a new GUID, `Created` timestamp).
- `User` uses a **composite key** `(OpenIdIssuer, OpenIdSubject)` — see `Infrastructure/Context.cs`.
- `Group` maintains a materialized `Path` (built with `Application/Utilities/PathUtilities`) used for hierarchical access checks in `GroupAccessService`.
- Register new `DbSet`s and any model configuration in `Infrastructure/Context.cs`.

### Configuration

- Env vars are loaded via `dotenv.net` in Development (`DotEnv.Load()` in `Program.cs`). See `API/.env.example`.
- Required backend env vars: `PUBLIC_OPENID_AUTHORITY`, `PUBLIC_OPENID_CLIENTID`, `OPENID_CLIENT_SECRET`, `PUBLIC_FRONTEND_URL`, `PUBLIC_API_URL`.
- JWT bearer auth validates against `PUBLIC_OPENID_AUTHORITY` with `PUBLIC_OPENID_CLIENTID` audience. The default authorization policy requires a `ClaimTypes.NameIdentifier` claim and denies anonymous access.
- CORS allows `PUBLIC_FRONTEND_URL` with credentials.
- OpenAPI is exposed at `/openapi/v1.json`; Scalar API reference is mapped by default.

## Frontend Conventions

- **Svelte 5 runes** (`$state`, `$props`, `$effect`, `$derived`) and `.svelte.ts` files for reactive stores (e.g. `lib/auth.svelte.ts`, `lib/state/group.svelte.ts`). Do not use legacy Svelte 4 stores/reactivity.
- **Auth**: `oidc-client-ts` `UserManager` (`lib/auth.svelte.ts`), `AuthSync.svelte` bootstraps the session, sets the `Authorization: Bearer` header on the generated client, and routes to `/login` or `/onboard` as needed. Authenticated pages live under `src/routes/app/`.
- **API client**: generated by `@hey-api/openapi-ts` into `src/lib/client/` from the running backend's OpenAPI document. **Do not hand-edit generated files** (`*.gen.ts`, `src/lib/client/**`). Regenerate with `bun run generate` (backend must be running on `http://localhost:5228`).
- **Validation**: Zod schemas generated alongside the client (`src/lib/client/zod.gen.ts`) are used client-side before submitting.
- **UI**: DaisyUI + Tailwind v4 (configured via `@tailwindcss/vite`); use `svelte-french-toast` (`toast.success` / `toast.error`) for user feedback. Styling lives in `src/routes/layout.css`.
- Use the `$lib` alias for imports.
- Package manager is **bun** (`bun.lock` present), though npm scripts work too.

## Common Commands

### Backend (from repo root)

```bash
dotnet build centraLM.sln
dotnet run --project API          # serves on http://localhost:5228 (see launchSettings.json)
```

There is currently **no test project**; if you add one, add it to `centraLM.sln`.

### Frontend (from `Frontend/`)

```bash
bun install
bun run dev            # vite dev server
bun run build          # production build (adapter-node)
bun run check          # svelte-check + TypeScript
bun run lint           # prettier --check . && eslint .
bun run format         # prettier --write .
bun run generate       # regenerate API client from OpenAPI
```

## Style Notes

- **No comments in any code you write** (see Prime Directive). Self-describing names over explanation.
- C#: 4-space indentation, file-scoped namespaces, `sealed` where possible, nullable enabled. Match the existing file's namespace to its folder (`Application.Groups.Commands`, `API.Endpoints`, …).
- Frontend: Prettier + `prettier-plugin-svelte` and `prettier-plugin-tailwindcss` (see `.prettierrc`); tabs for indentation. Run `bun run format` before committing.
- Keep the frontend generated client and Zod schemas in sync with the API whenever endpoints/DTOs change: rebuild/run the API, then `bun run generate`.

## Gotchas

- **Mediator, not MediatR** — the package is `Mediator.Abstractions` with the source generator; handlers implement `ICommandHandler<,>` / `IQueryHandler<,>` and endpoints resolve `IMediator`.
- **In-memory DB currently** — data does not persist across restarts until the Postgres provider is wired up. Don't assume migrations exist.
- **No migrations folder yet** — EF Core migrations have not been created.
- `RequireUser()` returns `401` when no app user exists for the authenticated OpenID identity (user not onboarded); onboarding is handled by `POST /users/me/register` from the frontend `/onboard` flow.
- `Project` and `VirtualKey` models exist and `CreateProjectCommand` is a stub (`NotImplementedException`); treat those areas as incomplete.
- Environment variables are read directly via `Environment.GetEnvironmentVariable(...)` at startup; missing values will surface as auth/CORS failures.
