# Current AI Development Architecture Audit

**Date:** 2026-09-21  
**Status:** Analysis only. Nothing was implemented, created, or installed during the audit except this document.  
**Purpose:** Handoff briefing for another model/person. This is an observed-state audit, not a generic Cursor recommendation.

## How to read this

This audit covers three things:

1. The current Cursor workspace: **foxbyte-labs** (`D:\repos\foxbyte-labs`)
2. A real product repo used as the example: **SCAR** (`C:\Users\msvn\source\repos\SCAR`)
3. The official Hulamin app template: **Template** (`C:\Users\msvn\source\repos\Template`)

**FoxDev** is the intended future AI development environment (rules, skills, commands, hooks, MCP). It does not exist yet.

What could not be inspected:

- Full Cursor Settings UI (models, memories, indexing, privacy)
- Whether Cursor auto-injects `CLAUDE.md` or Copilot instructions when SCAR is the open workspace
- Plugin enablement outside the audited Cursor session
- SCAR git remotes/history

Do not assume configuration exists because Cursor supports it. Only observed files and session capabilities are reported.

Secrets were found in some appsettings files. They are **not** copied here. Flag them; do not reproduce them.

---

## 1. Cursor rules

### Project-level Cursor rules

None exist.

| Location | Result |
|---|---|
| `foxbyte-labs/.cursor/rules` | Missing |
| `foxbyte-labs/.cursorrules` | Missing |
| `SCAR/.cursor/rules` | Missing |
| `SCAR/.cursorrules` | Missing |
| `Template/.cursor/` | Missing |
| `C:\Users\msvn\.cursor\rules` | Missing |

### Global / user rules

Cursor Settings personal-rules API returned **0 rules**.

The audited Cursor session still received three user rules in the agent prompt. They apply globally:

1. **Git commit safety**  
   Only commit when asked. Never update git config. Never force-push/hard-reset unless requested. Never skip hooks. Never amend except under strict conditions. Gather status/diff/log first. Do not commit secrets.

2. **Pull request workflow**  
   Use `gh`. Inspect status/diff/tracking/log. Push with `-u` if needed. PR body must include Summary and Test plan.

3. **Web UI verification**  
   When changing UI/layout/routing/client state, verify in the browser end-to-end. A screenshot is not verification. Check related routes and edge states.

### Files that look like rules but are not Cursor rules

These exist in SCAR and would influence Claude Code / GitHub Copilot, not necessarily Cursor, unless an agent reads them:

- `C:\Users\msvn\source\repos\SCAR\CLAUDE.md`
- `C:\Users\msvn\source\repos\SCAR\.github\copilot-instructions.md`

They were **not** injected into the foxbyte-labs Cursor session.

---

## 2. Agent instruction files

### foxbyte-labs

None of: `AGENTS.md`, `CLAUDE.md`, `README.md`, `CONTRIBUTING.md`, architecture docs, `.editorconfig`.

The public site has no written instructions for agents or developers.

### SCAR

**`CLAUDE.md` — most accurate existing briefing**

Written for Claude Code. Describes SCAR as a Hulamin Supplier Corrective Action Request system with two ASP.NET Core apps and Oracle.

It tells an agent:

- How to build/run: `dotnet build BES.SCAR.sln`, run API, run Web, run tests
- Architecture: Web never touches the DB; API is the only DB process
- Wolverine in `MediatorOnly` mode with method-parameter injection
- Features live in `Features/<Entity>/Commands` and `Queries`
- Hand-rolled `Mapper`, no AutoMapper
- Oracle `FG.setusername` before every `SaveChangesAsync`
- Azure AD auth, BES Menu ACL groups, email via internal API, SLA engine, Redis fallback, Scalar, Serilog `Log/SCAR.log`

This is the file to trust if an agent reads SCAR docs.

**`.github/copilot-instructions.md` — stale HPH/template instructions**

Written for GitHub Copilot. Describes a different product:

- MediatR instead of Wolverine
- AutoMapper `GeneralProfile`
- FluentValidation MediatR pipeline
- Generic repository
- BES.AKV
- Employee `[NotMapped]` HR derived fields
- Log files named `HPH.log`
- Admin/View roles instead of QA/SHE/Purchasing groups

This file actively conflicts with the SCAR codebase and with `CLAUDE.md`.

**`.editorconfig`**

UTF-8, CRLF, 4-space C#, braces on new lines for types/methods/control blocks. Editor formatting, not agent architecture.

**`azure-pipelines.yml`**

Not an AI file, but it encodes a wrong build story: `BES.HPH.API` / `BES.HPH.Web`, .NET 8 SDK, commented tests, leftover HPH names. An agent following the pipeline as documentation would publish the wrong projects on the wrong runtime.

**Missing in SCAR:** `AGENTS.md`, `README.md`, `CONTRIBUTING.md`, `.cursorignore`

### Template

No Cursor/agent files. Better *human* docs exist:

- `README.md` — how to pack/publish/install `dotnet new hulamin-template`
- `Templates/MainTemplate/DEVELOPMENT.md` — current generated-app conventions
- `Templates/MainTemplate/DEVELOPER-TOOLS.md` — local diagnostics, secrets policy, loopback-only developer tools
- `architecture.md` — **stale**; still says AutoMapper
- `IMPLEMENTATION-REPORT.md` — one-off usability release notes, not agent instructions

---

## 3. Project architecture

### foxbyte-labs (current Cursor workspace)

Small public site, not a backend product.

- Language: C#
- Framework: Blazor WebAssembly
- Target: `net10.0`
- Type: static WASM site for GitHub Pages (`foxbytelabs.co.za`)
- Layers: Pages / Components / Layout / Models / Services / wwwroot
- Data: no database. `ProjectService` loads a remote Fox’s Den manifest, then falls back to `wwwroot/data/projects.json`
- API: none
- Tests: none
- Deploy: `.github/workflows/deploy-pages.yml` on `main`

Useful as a Foxbyte public site. Not the template for Hulamin internal app work.

### SCAR (example product repo)

Internal Hulamin app for raising, reviewing, issuing, containing, and closing quality/SHE corrective actions against suppliers.

**Runtime**

- C# / ASP.NET Core
- `net10.0` on all seven projects
- Nullable and implicit usings enabled

**Projects**

| Project | Role |
|---|---|
| `BES.SCAR.Api` | Only process that talks to Oracle. JWT, Scalar, Wolverine, Serilog, observability |
| `BES.SCAR.Web` | Authenticated MVC frontend. HTTP client to API. ACL, Redis, DevExtreme |
| `BES.SCAR.Application` | CQRS handlers, interfaces, SLA, mapping, notifications |
| `BES.SCAR.Domain` | Entity classes only |
| `BES.SCAR.Persistence` | EF Core + Oracle, `ApplicationDbContext` partials, entity repositories |
| `BES.SCAR.Shared` | `Response<T>`, request/response DTOs, exceptions |
| `BES.SCAR.UnitTests` | xUnit; portal/mapper/command tests with fakes |

**Request flow**

```
Browser
  -> BES.SCAR.Web (MVC + DevExtreme)
       Auth: Azure AD OIDC
       ACL: BES.Security.ACL / BES Menu
       HTTP: named client "HPHApiClient"   # leftover HPH name
  -> BES.SCAR.Api (ASP.NET Core, /api/v1.0/*)
       Auth: Azure AD JWT
       Controller : BaseApiController
       Wolverine IMessageBus.InvokeAsync
  -> BES.SCAR.Application (Features/*/Commands|Queries)
       Handler method-parameter DI
       Mapper (hand-rolled partial)
  -> BES.SCAR.Persistence (entity repositories)
       ApplicationDbContext
       FG.setusername + audit columns
  -> Oracle
```

Web references only `BES.SCAR.Shared`. That boundary is real in the csproj.

**Notable packages**

- API: WolverineFx 5.13, Microsoft.Identity.Web 4.3, Scalar, Serilog 9, BES.Observability, BES.EmailService
- Web: DevExtreme 24.2.3, BES.Security.ACL 1.0.27, Redis, Polly, Identity.Web mixed 3.2/4.0, Serilog 8
- Persistence: EF Core 10.0.2 + Oracle.EntityFrameworkCore
- Shared: FluentValidation 12.1.1 (package present; no `AbstractValidator` classes found)
- BES.AKV is **not** referenced, despite Copilot instructions

**Database**

- Oracle via EF Core
- Every `SaveChangesAsync` calls `BEGIN FG.setusername(:userId); END;` and stamps audit columns (`LAST_REC_UPD_*`, portal `USERLASTUPDATED` / `DATELASTUPDATED`)
- CAR numbers: insert temp GUID, Oracle assigns `SUP_CAR_SIN`, then format `SCAR-YYYYMM-{SIN:D4}`

**API**

- URL versioning: `api/v{version}/[controller]`, default `1.0`
- Envelope: `Response<T>` with `Succeeded`, `Message`, `Errors`, `Data`
- Error middleware maps `ApiException` / `ValidationException` / `KeyNotFoundException`
- Scalar in Development
- CORS currently allows any origin/method/header with credentials
- Health check: `/health`

**Web**

- MVC + Razor + DevExtreme
- IIS path base `/BES.SCAR.Web`
- `BaseHttpService` unwraps `Response<T>`
- Typed services: `ICarFormDataService`, `ISlaDashboardService`
- Fallback authorization: every MVC route requires auth

**External systems**

- Azure AD
- BES Menu / ACL
- BES Email API
- EVP supplier portal file API
- Redis, with silent in-memory fallback
- BES.Observability / OpenTelemetry

**Tests**

- Real xUnit tests exist for portal mapper, drafts, supplier response, attachments, extensions
- `CLAUDE.md` still says the test project is a stub — outdated
- Pipeline test steps are commented out and point at unrelated `RA.*` projects

**Config / deploy**

- appsettings + User Secrets
- Secrets appear committed in Web appsettings (Azure AD client secret, Redis password). Do not copy them.
- Azure Pipelines, manual trigger, Jira issue ID required, agent `HUL-GIT2`
- Pipeline still names `BES.HPH.*` and installs .NET 8 while projects target net10.0
- Pipeline references `NuGet.config`; that file was not found in the SCAR tree

```
                    Azure AD
                       |
     +-----------------+------------------+
     | OIDC (Web)                         | JWT (API)
     v                                    v
+------------------+     HTTP      +------------------+
| BES.SCAR.Web     | ------------> | BES.SCAR.Api     |
| MVC/DevExtreme   |  HPHApiClient | /api/v1.0/*      |
| ACL + Redis      |               | Wolverine bus    |
+--------+---------+               +--------+---------+
         |                                  |
    BES Menu / ACL                    Application
    Email API                         Features/CQRS
    EVP files                         Mapper / SLA
                                              |
                                       Persistence
                                       EF Core Oracle
                                              |
                                           Oracle
                                      FG.setusername
```

### Template (official new-app shape)

`Hulamin.ProjectTemplates` 1.1.0. Install with `dotnet new hulamin-template`.

Generated apps are `Hulamin.ProjectName.*` on net10.0, with optional MVC and optional Prod Streams sample.

This is the intended BES shape for **new** work. SCAR is an older cousin, not a generated instance of this template.

Template code matches SCAR’s *real* CQRS pattern:

- Wolverine `MediatorOnly`
- `BaseApiController` + `IMessageBus.SendAsync`
- `Features/.../Commands|Queries` with static `Handle(...)`
- Hand-rolled `Mapper` partial, no AutoMapper
- `Response<T>`
- Oracle `FG.setusername`

Template is ahead of SCAR in platform packages:

- Wolverine **6.38** vs SCAR 5.13
- API: `BES.AKV`, `BES.AspNetCore.Hosting`, `BES.AspNetCore.Notifications`
- Web: `BES.Mvc.Shell` 1.4.0, `BES.AspNetCore.Redis`, DevExtreme **26.1.5**
- `global.json` pins SDK 10.0.100
- Real `NuGet.Config` (`nuget.org` + `CustomTools`)
- Pipeline uses correct project names and actually runs tests

Template vs SCAR differences that must not be collapsed:

| Topic | SCAR | Template |
|---|---|---|
| API route | `api/v1.0/...` | `api/v1/...` |
| Web UI | Custom MVC + DevExtreme + `BaseWebController` | `BES.Mvc.Shell` |
| Auth on API base controller | authenticated app | `[AllowAnonymous]` on `BaseApiController` |
| Key Vault package | not present | `BES.AKV` |
| Tests in CI | commented out / wrong names | `dotnet test` runs |
| Pipeline names | `BES.HPH.*`, .NET 8 | `Hulamin.ProjectName.*`, `global.json` |

`Templates/MainTemplate/architecture.md` still says AutoMapper. Ignore it. Trust the generated code and `DEVELOPMENT.md`.

---

## 4. Development conventions

Legend:

- **Documented** = written in a repo file
- **Pattern** = repeated in code
- **Inference** = interpretation

### Naming

- Documented: project names `BES.SCAR.*`; CAR number `SCAR-YYYYMM-{SIN:D4}`
- Pattern: Oracle-shaped entities (`CmSupplierCar`, `SUP_CAR_SIN`); feature folders match entities
- Leftover: HTTP client still named `HPHApiClient`; email service `HphEmailService`; SCAR pipeline `BES.HPH.*`
- Pattern vs docs: C# namespaces are mostly `SCAR.*`; project names are `BES.SCAR.*`; Web uses `BES.SCAR.Web`

### Folder structure

- Documented and used: `Features/<Entity>/Commands` and `Queries` (plural `Commands`, not Copilot’s `Command/`)
- Pattern: Persistence `Contexts/ApplicationDbContext.<Entity>.cs`; Shared `Requests/` `Responses/` `Wrappers/`
- Template sample: `Features/ProductStreams` is the scaffolding exemplar for new BES apps

### DI / async

- Documented: Wolverine injects handler parameters; repositories Scoped
- Pattern: primary constructors on API controllers/repos; Web uses classic constructors
- Pattern: `async Task` + `CancellationToken ct = default`; `AsNoTracking()` on reads
- Not found in SCAR: Copilot’s `GenericRepositoryAsync<T>`

### Errors / logging

- Documented: `Response<T>` success/failure constructors
- Pattern: API middleware converts exceptions to `Response<string>`
- Pattern: Web `BaseHttpService` catches, logs, returns default/`(false, data, error)` rather than throwing
- Documented in CLAUDE: Serilog `Log/SCAR.log`
- Conflict: Copilot still says `HPH.log`

### Mapping / DTOs

- Documented in CLAUDE and used: no AutoMapper; `Mapper` singleton + extra mapper files
- Conflict: Copilot says add mappings to `GeneralProfile`
- Employee in SCAR is a simple entity with `FullName`. Copilot’s long `[NotMapped]` HR table is not in the entity that was read

### Testing

- SCAR pattern: xUnit `[Fact]`, fakes, portal-focused tests
- Template pattern: SQLite-backed grid tests, Oracle SQL generation without a connection, opt-in live Oracle via `HULAMIN_TEST_ORACLE_CONNECTION`, Playwright smoke starter
- Template CI runs tests. SCAR CI does not

### Secrets / local tools (from Template, not SCAR)

`DEVELOPER-TOOLS.md` is the best written policy:

- Credentials in user secrets or environment variables
- Never expose `Acl:ApiKey` to browser code
- Developer tools only on direct loopback Development; Production/Staging/remote/forwarded must 404
- Setup diagnostics (`--check-setup`) must not print secrets
- Menu visibility is not authorization

### Git

- foxbyte-labs: short .NET gitignore; no commit-message convention
- SCAR: VisualStudio.gitignore; pipeline implies Jira IDs
- Global Cursor user rule: commit only when asked

---

## 5. AI / agent configuration observed

### Live MCP in the audited Cursor session

| Namespace | Status | Role |
|---|---|---|
| `cursor` | ready | goals, image generation |
| `cursor-app-control` | ready | user rules, plugins, project create |
| `cursor-ide-browser` | ready | Cursor-owned browser + CDP |
| `cursor-subscriptions` | ready | GitHub/Origin CI, PRs, Linear, Slack |
| `plugin-context7-plugin-context7` | ready | library docs |
| `plugin-devtools-for-agents-chrome-devtools` | ready | Chrome DevTools MCP |
| `plugin-playwright-playwright` | ready | Playwright browser MCP |
| `plugin-sonarqube-sonarqube` | **error** | discovery failed |

### Plugins cached on disk

Figma, SonarQube, DevTools for Agents, Playwright, Context7.

Figma was cached but **not** in the live MCP catalog or available-skills list for that session. Do not treat it as active.

### Skills

- User skill: `C:\Users\msvn\.cursor\skills\plan\SKILL.md` (`/plan`, plan-only, writes `PLAN.md`)
- Bundled Cursor skills: create-rule, create-skill, create-hook, create-subagent, canvas, review, bugbot, security-review, etc.
- Plugin skills: Context7, SonarQube suite, Chrome DevTools / a11y / LCP

### Subagents (built-in only)

generalPurpose, explore, cursor-guide, ci-investigator, bugbot, security-review, best-of-n-runner, docs-researcher

No custom SCAR/FoxDev/Template subagents.

### Commands / hooks

- `C:\Users\msvn\.cursor\commands` does not exist
- No repo `.cursor/commands`
- `C:\Users\msvn\.cursor\hooks` does not exist
- No project hooks in foxbyte-labs, SCAR, or Template

### Cursor user settings

`settings.json` only had theme/chime settings. No MCP or agent config there. No user-level `mcp.json` found.

---

## 6. Conflicts

1. **SCAR `CLAUDE.md` vs SCAR Copilot instructions**  
   Wolverine vs MediatR; hand-rolled Mapper vs AutoMapper; entity repos vs generic repo; `SCAR.log` vs `HPH.log`; QA/SHE/Purchasing vs Admin/View; User Secrets vs BES.AKV. Highest-risk AI conflict.

2. **`CLAUDE.md` vs actual tests**  
   File says stub. Repo has real xUnit files.

3. **SCAR pipeline vs SCAR projects**  
   Publishes `BES.HPH.*` on .NET 8. Projects are `BES.SCAR.*` on net10.0. `NuGet.config` missing.

4. **HPH leftover names in live SCAR code**  
   `HPHApiClient`, `Endpoints:HPHApi`, `HphEmailService`.

5. **Three browser automation stacks**  
   Cursor IDE browser + Chrome DevTools plugin + Playwright plugin, plus a user rule saying “verify in the browser”. No rule says which one to use.

6. **SonarQube skills present, MCP broken.**

7. **Template `architecture.md` vs template code**  
   Same AutoMapper lie as SCAR Copilot. Template *code* uses Wolverine + hand-rolled Mapper.

8. **Template platform vs SCAR platform**  
   Shell/AKV/Hosting/Wolverine 6 vs custom MVC/no AKV/Wolverine 5. A single “BES app” rule would mis-instruct SCAR pages.

9. **FluentValidation package without validators** in SCAR.

10. **CORS AllowAnyOrigin + credentials** on SCAR API. Hardening vs “don’t break IIS” tension.

11. **No shared FoxDev layer** across foxbyte-labs, SCAR, Template, RFX.

---

## 7. Current AI capability

### Explicitly configured

- Global user rules: git safety, PRs, web verification
- User `/plan` skill
- SCAR `CLAUDE.md` (accurate, Claude-oriented)
- SCAR Copilot instructions (present, largely stale)
- SCAR `.editorconfig`
- Template `DEVELOPMENT.md` / `DEVELOPER-TOOLS.md` (human docs, not Cursor rules)
- MCP: Context7, Chrome DevTools, Playwright, SonarQube (broken), Cursor browser
- Built-in subagents and bundled Cursor skills

### Available but not project-configured

- Ordinary Cursor coding
- Browser verification via three MCP browsers
- Context7 for Wolverine / EF Core / DevExtreme / Identity.Web docs
- `/plan` before coding
- Bugbot / security-review if asked
- foxbyte-labs is small enough to navigate without rules

### Missing

- Project Cursor rules for any of the three repos
- `AGENTS.md` as a Cursor-native instruction file
- One source of truth (Copilot/architecture.md must not contradict code)
- Custom skills for BES/SCAR feature scaffolding, Oracle audit, portal flows, ACL/email
- Custom subagents
- Commands
- Hooks (secrets, forbidden MediatR/AutoMapper, pipeline name check)
- Working SonarQube MCP
- A designated browser-verification path
- FoxDev that travels across foxbyte-labs, SCAR, RFX, and template-generated apps

---

## 8. What to grab from Template

Grab:

- `Templates/MainTemplate/DEVELOPMENT.md`
- `Templates/MainTemplate/DEVELOPER-TOOLS.md`
- ProductStreams CQRS sample as the scaffolding exemplar
- Test architecture (SQLite + SQL-gen + opt-in live Oracle + Playwright fixture)
- Pipeline / `global.json` / `NuGet.Config` as the CI reference for **new** apps
- Secrets and developer-tools policy

Do not grab blindly:

- `architecture.md` (stale AutoMapper)
- `IMPLEMENTATION-REPORT.md` / artifacts
- Shell page conventions into SCAR (SCAR does not use `BES.Mvc.Shell`)
- Template `api/v1` into SCAR (`api/v1.0`)
- Committed appsettings placeholders as a secrets pattern
- Suspicious trailing comment in `Hulamin.Templates.csproj` (do not copy; human should inspect)

Template does **not** currently ship any Cursor rules/skills/commands. Generating a new app from it still would not create FoxDev.

---

## 9. Proposed FoxDev structure

Do not implement yet. This is the recommended layout.

```
Global (Cursor user)
  rules/
    git / PRs / browser-verify-one-stack / never-commit-secrets
    BES Web has no DbContext
    Wolverine not MediatR; hand-rolled Mapper; Response<T>
    distrust HPH / MediatR / AutoMapper instruction files
  skills/plan
    already exists — keep as default pre-code step

Per product repo
  AGENTS.md
    short index: what this app is, how to run it, where truth lives
  .cursor/rules/
    architecture always-on
    web glob
    persistence glob
    tests glob
  .cursor/skills/
    add-feature / oracle-entity / portal-flow (SCAR)
    mvc-shell-page (template-generated apps only)
  .cursor/commands/
    /scar-new-entity, /scar-trace-car, /audit-ai-config
  .cursor/hooks/
    secrets, forbidden packages, pipeline project-name check

SCAR-specific overlay
  api/v1.0
  HTTP client name is HPHApiClient even though product is SCAR
  no BES.Mvc.Shell
  Wolverine 5
  existing BaseWebController ACL

New-template-app overlay
  api/v1
  BES.Mvc.Shell
  BES.AKV
  HybridCache consistency limits
  --check-setup
  developer tools loopback-only

Retirement
  rewrite or delete SCAR .github/copilot-instructions.md
  rewrite Template architecture.md
```

Instruction-file precedence to encode globally:

1. `.cursor/rules`
2. `AGENTS.md`
3. `CLAUDE.md`
4. Copilot instructions, treated as suspect if they mention HPH / MediatR / AutoMapper in a SCAR-like repo

Practical first cleanup:

1. Treat SCAR `CLAUDE.md` as source of truth for SCAR
2. Rewrite or remove SCAR Copilot instructions
3. Promote that truth into `AGENTS.md` + `.cursor/rules`
4. Use Template `DEVELOPMENT.md` / `DEVELOPER-TOOLS.md` for the **new-app** overlay, not as SCAR law

---

## 10. Final summary

### Current state

Cursor is a strong general coding agent with global habits (git/PR/browser) and marketplace plugins, but almost no project-level FoxDev layer.

- foxbyte-labs: small Blazor WASM site, zero AI instruction files
- SCAR: real clean-architecture .NET 10 system with two competing instruction files
- Template: better current BES golden path and human docs, still zero Cursor config

The accurate SCAR briefing is `CLAUDE.md`. The Copilot file is a stale HPH/MediatR/AutoMapper template.

### Already working well

- SCAR code structure is consistent: Web/API split, Features CQRS, `Response<T>`, entity repos, Oracle audit hook
- Template generated code uses the same CQRS pattern, on newer BES packages
- Template `DEVELOPMENT.md` and `DEVELOPER-TOOLS.md` are the best written conventions found
- Global git/PR/browser user rules are practical
- `/plan` skill already exists
- Context7 + browser MCP are useful
- SCAR portal xUnit tests are a real kernel; Template tests are the better CI model

### Duplication or problems

- Copilot instructions vs CLAUDE vs SCAR code
- Template `architecture.md` vs template code
- SCAR pipeline still builds HPH on .NET 8
- HPH names left in live SCAR HTTP client config
- Three browser tool stacks, no winner
- SonarQube plugin present, MCP broken
- `CLAUDE.md` test-stub claim is outdated
- Secrets in committed SCAR appsettings
- No `AGENTS.md` anywhere audited

### Missing capabilities

- Project Cursor rules
- One canonical instruction file per repo
- Feature-scaffolding skill
- Test/CI alignment for SCAR
- Secret and “don’t add AutoMapper/MediatR” hooks
- Designated browser path
- Working Sonar integration
- A FoxDev structure that travels across foxbyte-labs, SCAR, RFX, and template-generated apps

### Source-of-truth map

| Source | Use |
|---|---|
| SCAR `CLAUDE.md` + actual SCAR code | Canonical rules for SCAR |
| SCAR Copilot file | Do not use |
| Template `DEVELOPMENT.md` / `DEVELOPER-TOOLS.md` | Canonical conventions for new BES apps |
| Template CQRS sample | Scaffolding exemplar |
| Template `architecture.md` | Do not use until rewritten |
| foxbyte-labs code | Canonical for the public WASM site only |
| Cursor user rules | Keep git/PR/browser habits |
