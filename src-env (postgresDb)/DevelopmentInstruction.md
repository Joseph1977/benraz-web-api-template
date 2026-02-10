# Project Development Guide

## Purpose of the Document

The purpose of this document is to serve as a comprehensive guide for developers, contributors, and AI coding assistants working on any microservice in this project. It outlines the project's structure, best practices, coding standards, development workflow, and architectural decisions to ensure **exact consistency**, maintainability, and scalability across the entire codebase.

> **Important**: This document is generic. Throughout the document:
> - `[ServiceName]` = The actual service prefix (e.g., `DocumentManager`, `DueDiligence`, `Prompt`).
> - `[Entity]` = The singular entity name in PascalCase (e.g., `Document`, `ReportCategory`, `AtomicPrompt`).
> - `[Entities]` = The **plural** entity name in PascalCase (e.g., `Documents`, `ReportCategories`, `AtomicPrompts`).
> - Always substitute with the **actual names** for the specific microservice you are working on.

---

## Project Structure Overview

This service follows a **Domain-Driven Design (DDD) layered microservice architecture** with a clear separation of concerns across multiple projects:

### [ServiceName].WebApi – Main ASP.NET Core Web API project
- The main entry point of the microservice. Responsibilities include:
  - **HTTP Endpoints (Controllers)**: Expose RESTful APIs.
  - **Request/Response Mapping**:
    - Uses AutoMapper to convert domain entity models to view models and vice versa.
    - Domain entity models are **never** returned directly in API responses.
  - **Swagger Integration**:
    - Web UI tool to view, test, and interact with API endpoints. Access via `{domain}/swagger` in the browser.
  - **Logging**:
    - Integrated NLog for structured logging.
    - Use for information log and error log.
    - `nlog.config` file contains configuration for NLog and is located in the WebApi project root.
  - **Authorization**:
    - Uses Claim-based policies via `[Authorize(POLICYNAME)]` attribute.
    - Wrapped in `#if EnableAuthorization` / `#endif` preprocessor directives to allow disabling during local debugging.
    - **Claim naming format**: `{servicename-lowercase}-{controller-action-lowercase}` (e.g., `documentmanager-documents-read`, `duediligence-report-type-add`).
    - **Policy naming format**: `{controller-action-lowercase}` with hyphens (e.g., `documents-read`, `report-type-add`).
    - **Claim/Policy constant naming**: `UPPER_SNAKE_CASE` (e.g., `DOCUMENTS_READ`, `REPORT_TYPE_ADD`).
    - To add a new endpoint with authorization:
      1. Add the claim constant in `[ServiceName]Claims` class (inside `[ServiceName].Domain/Authorization/`).
      2. Add the policy constant in `[ServiceName]Policies` class (inside `[ServiceName].WebApi/Authorization/`).
      3. Register the policy in `Startup.cs` → `AddAuthorization` method:  
         `options.AddClaimsPolicy([ServiceName]Policies.CONSTANT_NAME, [ServiceName]Claims.CONSTANT_NAME);`
      4. Update claims script and Excel sheet (in `/docs` folder).
  - **Authentication Toggle**:
    - Uses `#if EnableAuthorization` preprocessor directive to conditionally apply `[Authorize]` attributes.
    - Always build in **Release mode** before committing code (Release mode enables `EnableAuthorization`).
  - **Environment Configurations**:
      - See `Environment Configurations (postgresDb)` section.

  - **Special Controllers** (always present, do not remove):
    - `JobsController`: 
        - Exposes API endpoints to run background tasks triggered by an external scheduler.
        - Includes a sample Execute endpoint for reference.

    - `ITController`: 
        - Includes an `IsAlive` endpoint that returns HTTP 200 (OK). 
        - This helps to easily check if the microservice is running and healthy.
  - **Documentation**:
    - Every method/property must have XML summary (`<summary>`) and `<param>` comments ending with a period (`.`).

  - **Key Files in WebApi project**:
    - `Startup.cs`: Configures DI, middleware, authorization, DB context, AutoMapper profiles, API versioning.
    - `Program.cs`: Application entry point.
    - `[ServiceName]AutoMapperProfile.cs`: WebApi-level AutoMapper profile class (maps ViewModels ↔ Entities and Queries).
    - `DesignTimeDbContextFactory.cs`: Used by EF Core migrations tooling at design time.
    - `nlog.config`: NLog logging configuration.
    - `Authorization/[ServiceName]Policies.cs`: Policy constants.
    - `Controllers/`: All API controllers.
    - `Models/`: ViewModel classes organized in subfolders by plural entity name.
    - `Extensions/`: Startup extension methods (e.g., Swagger configuration).

### [ServiceName].Domain – Domain Layer (DDD)
- Contains **core business logic** following Domain-Driven Design principles:
  - **Domain entity models** (aggregate roots and lookup/reference entities).
  - **Domain service interfaces and implementations** (business logic).
  - **Repository interfaces** (data access contracts — implementations live in the EF layer).
  - **Query models** for filtering/paging (used by GetAll endpoints).
  - **Authorization** folder containing `[ServiceName]Claims.cs` and `[ServiceName]Roles.cs`.
  - **General settings** class: `[ServiceName]GeneralSettings.cs` in the Domain root.
  - **Optional**: `[ServiceName]DomainAutoMapperProfile.cs` for internal domain-to-domain mappings (e.g., entity to history entity).

- **Domain Folder Structure (DDD — one folder per entity/aggregate)**:
  - Each entity gets its own **PascalCase plural-named** folder under the Domain project root (e.g., `Documents/`, `ReportCategories/`, `AtomicPrompts/`, `Images/`).
  - The folder name is always the **plural form** of the entity name.
  - Files inside each entity folder:
  
    | File | Description |
    |------|-------------|
    | `[Entity].cs` | Entity class definition (singular name). |
    | `I[Entity]Service.cs` | Service interface (singular name). |
    | `[Entity]Service.cs` | Service implementation (singular name). |
    | `I[Entity]Repository.cs` OR `I[Entities]Repository.cs` | Repository interface. |
    | `[Entities]Query.cs` *(if GetAll with filter is needed)* | Query/filter model class (plural name). |
    | `[Entities]QueryParameters.cs` *(if GetAll with filter is needed)* | Sortable fields enum (plural name). |

  > **Note**: Some simple entities (lookup/reference tables) may only have the entity class and repository interface — no service, no query model.

### [ServiceName].EF – Data Access Layer (Entity Framework Core)
- Responsible for:
  - Implementing repository interfaces defined in the Domain layer.
  - Database operations via `DbContext`.
  - Entity type configurations (table mappings, keys, relationships, constraints).
  - Seed data configurations.
  - Database migration services.

- **EF Folder Structure**:

    | Folder/File | Description |
    |-------------|-------------|
    | `[ServiceName]DbContext.cs` | The `DbContext` class. Contains `DbSet<>` properties for each entity. Uses `ApplyConfigurationsFromAssembly()` in `OnModelCreating`. |
    | `Repositories/` | Repository implementations. One file per entity. |
    | `Configurations/` | EF Core `IEntityTypeConfiguration<T>` classes. One file per entity. Defines table name, keys, columns, relationships. |
    | `DataConfigurations/` | Seed data classes using `IEntityTypeConfiguration<T>` with `builder.HasData(...)`. Used for lookup/reference tables. |
    | `Services/` | Contains `IDbMigrationService.cs` interface and `[ServiceName]DbMigrationService.cs` implementation. |
    | `Migrations/` | EF Core auto-generated migration files. |
    | `ModelBuilderExtensions.cs` *(optional)* | Extension methods for `ModelBuilder` (e.g., `ConfigureDateTimeToUtc`). |

### [ServiceName].Domain.Tests – Unit Tests for Domain
- Tests business logic and domain services for the Domain layer.
- Uses **NUnit** test framework and **FluentAssertions**.

### [ServiceName].EF.Tests – Unit Tests for EF Layer
  - Validates entity configurations, repository logic, and EF Core operations.
  - Uses **NUnit** test framework and **FluentAssertions**.
  - **Structure**:
    - `[ServiceName]RepositoryTestsBase.cs`: Generic abstract base class `[ServiceName]RepositoryTestsBase<TKey, TEntity, TRepository>` providing standard CRUD test methods (Add, GetById, Change/Update, Delete). All entity test classes inherit from this.
    - `[ServiceName]RepositoryReferenceTableTestsBase.cs` *(optional)*: Base class for lookup/reference table repository tests.
    - `Common/TestEntityFactory.cs`: Static factory class for creating default test entities.
    - Entity-specific test files: `[Entity]RepositoryTests.cs` — inherits from the base, overrides `CreateDefaultEntity()`, `CreateRepository()`, `ClearDataAsync()`.

### [ServiceName].WebApi.IntegrationTests – Integration Tests for Web API
- End-to-end integration tests for controller endpoints.
- Uses **NUnit** test framework and **FluentAssertions**.
- **Structure**:
    - `Config.cs`: `[SetUpFixture]` class that creates `TestServer`, `HttpClient`, and `DbContext` for the test run.
    - `StartupStub.cs`: Inherits from `Startup`, overrides `AddAuthorization` to bypass real JWT validation for testing.
    - `ControllerTestsBase.cs`: Abstract base class providing `HttpClient`, `DbContext`, and helper methods (`SendAsync`, `PutAsync`, `GetResponse<T>`).
    - `Common/TestEntityFactory.cs`: Static factory class for creating default test ViewModels/request models.
    - `Configuration/Constants.cs`: API version and base URL constants (e.g., `public const string ApiVersion = "/v1/";`).
    - Entity-specific test files: `[Entity]ControllerTests.cs` — inherits `ControllerTestsBase`, implements `CanAddEntityAsync`, `CanUpdateEntityAsync`, `CanGetAllEntitiesAsync`, `CanGetEntityByIdAsync`, etc.

---

## Entity Types and Base Classes

### Two Types of Domain Entities

#### 1. **Aggregate Root Entities** (Full CRUD Entities)
- Inherit from `AggregateRootBase<Guid>` (from `Benraz.Infrastructure.Common.EntityBase`).
- `AggregateRootBase<Guid>` provides these built-in properties:
  - `Guid Id` — Primary key.
  - `DateTime CreateTimeUtc` — Creation timestamp (UTC).
  - `DateTime UpdateTimeUtc` — Last update timestamp (UTC).
- These entities have a full set: entity class, service interface, service implementation, repository interface, and optionally query/filter models.
- Repository interface inherits from `IRepository<Guid, [Entity]>` (from `Benraz.Infrastructure.Common.Repositories`).
- Repository implementation inherits from `RepositoryBase<Guid, [Entity], [ServiceName]DbContext>` (from `Benraz.Infrastructure.EF`).

#### 2. **Lookup/Reference Entities** (Read-Only or Seed-Data Entities)
- Inherit from `IEntity<TEnum>` (from `Benraz.Infrastructure.Common.EntityBase`) where `TEnum` is an `enum` representing the entity's ID.
- Used for static reference data (e.g., status types, category types) that is seeded via `DataConfigurations`.
- Typically have only: entity class + repository interface (no service class, no query model).
- Repository interface inherits from `IQueryRepository<TEnum, [Entity]>` (from `Benraz.Infrastructure.Common.Repositories`).
- Repository implementation inherits from `QueryRepositoryBase<TEnum, [Entity], [ServiceName]DbContext>` (from `Benraz.Infrastructure.EF`).

---

## Environment Configurations (`postgresDb`)

### 1. **Environment Variables Implementation** (`postgresDb`)
**Best for:** Cloud-native deployments with containerization and CI/CD pipelines

**Features:**
- Entire configuration managed via environment variables (no reliance on `appsettings.json`)
- Supports `.env` files for different environments and regions
  - For `non-local` environments, use environment-specific files inside the `.env/` directory
  - For `local` development, configuration is managed via `launchSettings.json`
- Secrets and sensitive settings are automatically injected from Cloud Key Vault to environment variables
- Supports a `PostgreSQL` database provider.

- **Choose:**
- **`postgresDb`** - For **PostgreSQL** databases using **environment variable-based configuration**

**When to use:**
- Use this implementation when:
  - Managing secrets with a Cloud Key Vault or a secure secrets provider
  - DevOps CI/CD pipelines
  - Multi-environment deployments (`dev`, `qa`, `staging`, `production`)
  - When using Cloud Key Vault for secrets management
  
---


## Folder Structure

Use existing projects like:

- `[ServiceName].Domain`
- `[ServiceName].WebApi`
- `[ServiceName].EF`
- `[ServiceName].EF.Tests`
- `[ServiceName].Domain.Tests`
- `[ServiceName].WebApi.IntegrationTests`

> **Do NOT** create placeholder folders. Always use the actual project name.

## Our NuGet Library: 

## **`benraz-infra-nugets`**
   -Shared packages across all services:
   - Repository: https://github.com/Joseph1977/benraz-infra-nugets
    ### Common:
     - Use for common Enums, constants, models, static class, helpers.
    
    ### Domain
     - Use for Domain-specific models/constants.
    
    ### Gateways (HTTP Client Wrappers): 
     - HTTP Client Wrappers for Internal-Service Communication.
     - Gateway Usage Guidelines:
        - To integrate a new service gateway, follow the structure and standards below:
        - Location:
           - Create a folder inside:
              `benraz-infra-nugets/Benraz.Infrastructure.Gateways/Benraz[ServiceName]` for example (e.g., `BenrazAssets/`)
           - Folder Structure (inside `Benraz[ServiceName]`):
           - `Entities/`: DTO models
 	      - Contains DTO models used internally within the gateway.
           - `Messages/`: Request and Response models
              - Contains Request and Response models used for API communication.
              - `Request model`:
                - Must inherit from `TargetServiceRequestBase.cs`
                - Encapsulates common metadata such as `AccessToken`.
              - `Response model`:
		- Must inherit from `HttpResponseBase.cs`
		- Provides standard HTTP response behavior.
           - `ITargetServiceGateway.cs`:
	     - Interface defining all required API methods for the service.
           - `TargetServiceEndpoints.cs`:
             - Centralized static class defining all API endpoint paths for the service.
             - Keeps endpoint definitions consistent and maintainable.
           - `TargetServiceGateway.cs`:
             - Implements `ITargetServiceGateway.cs`
             - Handles actual HTTP communication using configured clients.
             - Follow existing gateway implementations as reference.
           - `TargetServiceGatewaySettings.cs`:
             - Configuration/settings class for service-specific options like base URL, timeouts, etc.
           - `TargetServiceRequestBase.cs`:
             - Base class for all request models sent to the service.
             - Inherits from `HttpRequestBase.cs`
             - Responsibilities:
               - Encapsulate common metadata (e.g., `AccessToken`)
               - Support abstraction for HTTP transport

## **`Benraz.Infrastructure`**:
   ### Benraz.Infrastructure.Web:
     - Handles web-specific authorization concerns, especially user identity retrieval in web requests.
     - Provides `AddClaimsPolicy` extension method for authorization setup.
   ### Benraz.Infrastructure.Common: 
     - Provides common infrastructure abstractions and base types for access control and entity management, supporting both web and domain layers.
     - Contains `AggregateRootBase<T>`, `IEntity<T>`, `IRepository<TKey, TEntity>`, `IQueryRepository<TKey, TEntity>`, `Page<T>`, `DomainException`.
   ### Benraz.Infrastructure.Domain:
     - Domain layer containing common domain entities.
   ### Benraz.Infrastructure.EF:
     - Contains `RepositoryBase<TKey, TEntity, TDbContext>` and `QueryRepositoryBase<TKey, TEntity, TDbContext>`.
     - Provides `ToPageAsync()` extension method for paging.
     - Provides `AndAlso()` and `OrElse()` expression tree extension methods for combining filter predicates.
   ### Benraz.Infrastructure.Gateways:
     - Internal and 3rd-party gateway implementations.
   ### Benraz.Infrastructure.Authorization:
     - Authorization services and JWT token management.
   ### Benraz.Infrastructure.Files: 
     - Cross-platform file management services (Azure Blob Storage support, Google Cloud Storage integration, FTP support).
   ### Benraz.Infrastructure.Phone:
     - Phone/SMS services and management.

   - Repository: https://github.com/Joseph1977/benraz-infra-nugets

## Technical Constraints / Limitations
- To ensure consistency, maintainability, and adherence to architectural principles, the following technical constraints apply:
- **Framework & Language**:
  - Target Framework: .NET 8
  - Language Version: C# 12
- **Architecture Rules**:
  - **No direct database access in WebAPI**
    - All data access must go through the domain layer (services → repositories).
  - **No domain logic in controllers**
    - Controllers must delegate all processing to domain services layer.
  - **No entity models in API responses**
    - Always map to ViewModels via AutoMapper before returning.
- **Library Usage**:
  - Use only approved libraries, including:
    - EF Core for data access (PostgreSQL via Npgsql)
    - AutoMapper for object mapping
    - NUnit for testing
    - FluentAssertions for test assertions
  - Any third-party libraries must be approved before use.

## Naming Conventions

| Element                    | Convention                          | Example                                 |
|--------------------------- |-------------------------------------|-----------------------------------------|
| Files/Classes              | PascalCase                          | `ReportCategory.cs`                     |
| Interfaces                 | Prefix with `I`                     | `IReportCategoryService`                |
| Methods                    | PascalCase                          | `GetByIdAsync`                          |
| Variables                  | camelCase                           | `reportCategoryViewModel`               |
| Properties                 | PascalCase                          | `DisplayName`                           |
| Domain Folders (entities)  | PascalCase, **Plural**              | `ReportCategories/`, `Documents/`       |
| WebApi Model Folders       | PascalCase, **Plural**              | `Models/ReportCategories/`              |
| Constants (Claims/Policies)| UPPER_SNAKE_CASE                    | `DOCUMENTS_READ`, `REPORT_TYPE_ADD`     |
| Enums                      | PascalCase                          | `ReportCategoryTypeCode`                |
| Enum Members               | PascalCase                          | `DocumentName = 1`                      |
| Namespaces                 | `[ServiceName].[Layer].[Entities]`  | `DueDiligence.Domain.ReportCategories`  |

### Additional Naming Rules:
- **Entity class**: Singular PascalCase (e.g., `ReportCategory`, `Document`, `AtomicPrompt`).
- **Entity folder**: **Plural** PascalCase (e.g., `ReportCategories/`, `Documents/`, `AtomicPrompts/`).
- **Controller class**: **Plural** entity name + `Controller` (e.g., `ReportCategoriesController`, `DocumentsController`).
- **Service class/interface**: **Singular** entity name + `Service` (e.g., `ReportCategoryService`, `IReportCategoryService`).
- **Repository class/interface**: Can be **singular or plural** entity name + `Repository` — follow the existing convention in the specific service (e.g., `IReportCategoryRepository`, `IDocumentsRepository`).
- **Query class**: **Plural** entity name + `Query` (e.g., `ReportCategoriesQuery`, `DocumentsQuery`, `AtomicPromptQuery`).
- **QueryParameters enum**: **Plural** entity name + `QueryParameters` (e.g., `ReportCategoriesQueryParameters`, `DocumentsQueryParameters`).
- **ViewModel classes**: `Add[Entity]ViewModel`, `Update[Entity]ViewModel`, `[Entity]ViewModel` (for output/read), `GetAll[Entities]ViewModel` (optional, for GetAll input).
- **AutoMapper profile**: `[ServiceName]AutoMapperProfile` in WebApi, `[ServiceName]DomainAutoMapperProfile` in Domain (optional).
- **Claims class**: `[ServiceName]Claims` (in Domain/Authorization/).
- **Policies class**: `[ServiceName]Policies` (in WebApi/Authorization/).
- **Roles class**: `[ServiceName]Roles` (in Domain/Authorization/).
- **DbContext class**: `[ServiceName]DbContext` (in EF project root).
- **General settings**: `[ServiceName]GeneralSettings` (in Domain project root).
- **Configuration class**: `[Entity]Configuration` (in EF/Configurations/).
- **Data configuration class**: `[Entity]DataConfiguration` (in EF/DataConfigurations/).
- **Migration service**: `[ServiceName]DbMigrationService` (in EF/Services/).

### Code Style Rules:
- Braces `{}` **always on a new line** (Allman style).
- Use `var` **only if the type is obvious** from the right-hand side.
- Every method/property **must** have XML summary (`<summary>`) and `<param>` comments ending with a period (`.`).
- File-scoped namespaces (`namespace X;`) are acceptable and used in newer files.

## Async/Await Conventions

- All async methods must have `Async` suffix.
- Use `Task<T>` return type instead of `void`.
- Always use `await` keyword — never fire-and-forget.

**✅ Correct:**
```csharp
public async Task<User> GetUserAsync(Guid userId)
{
    return await _userService.GetByIdAsync(userId);
}
```

**❌ Incorrect:**

```csharp
public async Task GetUser(Guid userId)
{
    _userService.GetByIdAsync(userId); // No await!
}
```

## Development Rules / Guidelines

### Adding New Features

1. Define entity/interface/service in Domain layer.
2. Implement repository in EF layer.
3. Add EF configuration and data configuration (if lookup).
4. Register the `DbSet<>` in the `DbContext`.
5. Add endpoint in WebApi controller.
6. Add ViewModel classes in WebApi/Models.
7. Add AutoMapper mappings in the WebApi AutoMapper profile.
8. Wire up DI in `Startup.cs` → `AddServices` method.
9. Add Authorization policy & claims.
10. Add tests in test projects.

### Copilot Task Template – Add New Aggregate Root Entity

> In the examples below, `[Entity]` is the singular entity name (e.g., `Setting`, `ReportCategory`), and `[Entities]` is the plural form (e.g., `Settings`, `ReportCategories`).

#### Domain: `[ServiceName].Domain`
- Add folder: `[Entities]/` (plural PascalCase name).
- Files inside the folder:

  - **`[Entity].cs`**: Entity class definition.
     - Must inherit from `AggregateRootBase<Guid>` (from `Benraz.Infrastructure.Common.EntityBase`).
     - `AggregateRootBase<Guid>` already includes: `Id`, `CreateTimeUtc`, `UpdateTimeUtc` properties — **do not re-declare them**.
     - Add all entity-specific properties with XML documentation.
     - Navigation properties should be `virtual` (e.g., `public virtual ReportType ReportType { get; set; }`).

  - **`I[Entity]Service.cs`**: Service interface.
     - Define methods: `AddAsync`, `UpdateAsync`, `GetByIdAsync`, `GetPageByQuery`, `DeleteAsync` (as needed).
     - Use `Task<T>` return types for all methods.

  - **`[Entity]Service.cs`**: Service implementation.
     - Implements `I[Entity]Service`.
     - Constructor receives `I[Entity]Repository` and `ILogger<[Entity]Service>` via DI.
     - In `AddAsync`: Assign `Id = Guid.NewGuid()` before saving.
     - In `UpdateAsync`: Use repository's `ChangeAsync` method.
     - Wrap operations in `try-catch` blocks. In the catch block:
       - Log with `_logger.LogError(ex, "...")`.
       - Throw `new DomainException(message, message)` (from `Benraz.Infrastructure.Common.Exceptions`).

  - **`I[Entity]Repository.cs`** (or `I[Entities]Repository.cs`): Repository interface.
     - Must inherit from `IRepository<Guid, [Entity]>` (from `Benraz.Infrastructure.Common.Repositories`).
     - Add `Task<Page<[Entity]>> GetPageByQuery([Entities]Query query)` if filtering is needed.

  - **Filter Support for GetAll Endpoints** (required for entities with GetAll):
    - Create the following files in the **same entity folder**:
      - **`[Entities]Query.cs`**: Query model class for filtering/paging.
        - Include entity-specific filter properties as **nullable** types.
        - **Must always include** these standard paging/sorting/filtering properties:
          ```csharp
          public int PageNo { get; set; }
          public int PageSize { get; set; }
          public bool Any { get; set; }
          public [Entities]QueryParameters? SortBy { get; set; }
          public bool SortDesc { get; set; }
          public DateTime? CreateTimeUtcRangeFrom { get; set; }
          public DateTime? CreateTimeUtcRangeTo { get; set; }
          public DateTime? UpdateTimeUtcRangeFrom { get; set; }
          public DateTime? UpdateTimeUtcRangeTo { get; set; }
          ```
        - Every property must have XML documentation comments.

      - **`[Entities]QueryParameters.cs`**: Enum defining sortable entity properties.
        - **Must always include** `CreateTimeUtc` and `UpdateTimeUtc` as sortable fields.
        - Members are numbered starting from `1`.
        - Example:
          ```csharp
          public enum [Entities]QueryParameters
          {
              Name = 1,
              DisplayName = 2,
              // ... more sortable fields ...
              CreateTimeUtc = N,
              UpdateTimeUtc = N+1
          }
          ```

#### Copilot Task Template – Add New Lookup/Reference Entity

- Add folder: `[Entities]/` in the Domain project (plural PascalCase name).
- Files inside:
  - **`[Entity].cs`**: Entity class — inherits `IEntity<[EntityEnum]>` (where `[EntityEnum]` is an enum from the shared enums library).
    - Properties: `[EntityEnum] Id`, `string Name`, `string Description` (and any other reference fields).
  - **`I[Entity]Repository.cs`** (or `I[Entities]Repository.cs`): Repository interface — inherits `IQueryRepository<[EntityEnum], [Entity]>`.
- In EF project:
  - `Configurations/[Entity]Configuration.cs`: Table mapping and key configuration.
  - `DataConfigurations/[Entity]DataConfiguration.cs`: Seed data using `builder.HasData(...)`.
  - `Repositories/[Entity]Repository.cs` (or `[Entities]Repository.cs`): Inherits `QueryRepositoryBase<[EntityEnum], [Entity], [ServiceName]DbContext>`.

#### EF Layer: `[ServiceName].EF`

- **`[ServiceName]DbContext.cs`** (root of EF project):
  - Add `DbSet<[Entity]>` property for the new entity.
  - `OnModelCreating` already uses `ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof([ServiceName]DbContext)))` — no changes needed.

- **Folder: `Repositories/`**
  - Add `[Entity]Repository.cs` (or `[Entities]Repository.cs`):
  - Implementation:
    ```csharp
    public class [Entity]Repository : 
        RepositoryBase<Guid, [Entity], [ServiceName]DbContext>, 
        I[Entity]Repository
    {
        private readonly [ServiceName]DbContext _context;

        public [Entity]Repository([ServiceName]DbContext context) : base(context)
        {
            _context = context;
        }

        // GetPageByQuery implementation (see Filtering section below)
        // GetQuery override (see below)
        // ApplyFiltering, ApplySorting private methods (see below)
    }
    ```

- **Folder: `Configurations/`**
  - Add `[Entity]Configuration.cs`:
    ```csharp
    public class [Entity]Configuration : IEntityTypeConfiguration<[Entity]>
    {
        public void Configure(EntityTypeBuilder<[Entity]> builder)
        {
            builder.ToTable("[Entities]");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasColumnName("[Entity]Id").HasColumnType("uuid");
            builder.Property(x => x.CreateTimeUtc).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            builder.Property(x => x.UpdateTimeUtc).HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");
            // Configure relationships, indexes, etc.
        }
    }
    ```

- **Folder: `DataConfigurations/`** *(only for lookup/reference entities)*
  - Add `[Entity]DataConfiguration.cs` with `builder.HasData(...)` to seed reference data.

- **Folder: `Services/`** *(already exists — contains migration service)*
  - `IDbMigrationService.cs` and `[ServiceName]DbMigrationService.cs` — no changes needed for new entities.

#### Repository Filtering and Sorting Pattern (inside Repository)

The `GetPageByQuery` method in the repository follows this exact pattern:

```csharp
public async Task<Page<[Entity]>> GetPageByQuery([Entities]Query query)
{
    if (query == null) throw new ArgumentNullException(nameof(query));

    var dbQuery = GetQuery();
    dbQuery = ApplyFiltering(dbQuery, query);
    dbQuery = ApplySorting(dbQuery, query);

    var totalCount = await dbQuery.CountAsync();
    query.PageSize = query.PageSize < 0 ? totalCount : query.PageSize;

    dbQuery = dbQuery.Skip(query.PageSize * (query.PageNo - 1)).Take(query.PageSize);

    var page = await dbQuery.ToPageAsync(0, query.PageSize);
    page.PageNo = query.PageNo;
    page.TotalCount = totalCount;

    return page;
}
```

**Private `ApplyFiltering` method**:
```csharp
private IQueryable<[Entity]> ApplyFiltering(IQueryable<[Entity]> dbQuery, [Entities]Query query)
{
    var filters = GetFilterings(query);
    Expression<Func<[Entity], bool>> filter = null;

    if (filters.Any())
    {
        // query.Any == true → OR logic; query.Any == false → AND logic
        filter = filters.Aggregate((x, y) => !query.Any ? x.AndAlso(y) : x.OrElse(y));
        dbQuery = dbQuery.Where(filter);
    }

    return dbQuery;
}
```

**Private `GetFilterings` method** — builds a list of expression predicates:
```csharp
private IEnumerable<Expression<Func<[Entity], bool>>> GetFilterings([Entities]Query query)
{
    var predicates = new List<Expression<Func<[Entity], bool>>>();

    // String filters: case-insensitive Contains
    if (!string.IsNullOrWhiteSpace(query.Name))
        predicates.Add(x => x.Name != null && x.Name.ToLower().Contains(query.Name.ToLower()));

    // Guid filters: null/empty check
    if (!IsGuidNullOrEmpty(query.SomeId))
        predicates.Add(x => x.SomeId == query.SomeId);

    // Boolean filters
    if (query.IsActive.HasValue)
        predicates.Add(x => x.IsActive == query.IsActive);

    // Date range filters (From + To paired)
    if (query.CreateTimeUtcRangeFrom.HasValue && query.CreateTimeUtcRangeTo.HasValue)
        predicates.Add(x => x.CreateTimeUtc.Date >= query.CreateTimeUtcRangeFrom.Value.Date 
                         && x.CreateTimeUtc.Date <= query.CreateTimeUtcRangeTo.Value.Date);

    if (query.UpdateTimeUtcRangeFrom.HasValue && query.UpdateTimeUtcRangeTo.HasValue)
        predicates.Add(x => x.UpdateTimeUtc.Date >= query.UpdateTimeUtcRangeFrom.Value.Date 
                         && x.UpdateTimeUtc.Date <= query.UpdateTimeUtcRangeTo.Value.Date);

    // Enum array filters
    if (query.StatusCodes != null && query.StatusCodes.Any())
        predicates.Add(x => query.StatusCodes.Contains(x.StatusCode));

    return predicates;
}
```

**Private `ApplySorting` and `GetSorting` methods**:
```csharp
private IQueryable<[Entity]> ApplySorting(IQueryable<[Entity]> dbQuery, [Entities]Query query)
{
    var sorting = GetSorting(query.SortBy);
    dbQuery = !query.SortDesc ? dbQuery.OrderBy(sorting) : dbQuery.OrderByDescending(sorting);
    return dbQuery;
}

private static Expression<Func<[Entity], object>> GetSorting([Entities]QueryParameters? parameter)
{
    return parameter switch
    {
        [Entities]QueryParameters.Name => x => x.Name,
        // ... map each enum member to its property ...
        [Entities]QueryParameters.CreateTimeUtc => x => x.CreateTimeUtc,
        [Entities]QueryParameters.UpdateTimeUtc => x => x.UpdateTimeUtc,
        _ => x => x.UpdateTimeUtc  // Default sort is always UpdateTimeUtc
    };
}
```

**Private `GetQuery` override** (base query with optional parameters):
```csharp
private IQueryable<[Entity]> GetQuery(bool includeProperties = true, bool asNoTracking = true)
{
    var query = asNoTracking ? base.GetQuery().AsNoTracking() : base.GetQuery();

    if (includeProperties)
        query = IncludeProperties(query);

    return query;
}

private IQueryable<[Entity]> IncludeProperties(IQueryable<[Entity]> query)
{
    // Add .Include() calls for navigation properties here
    return query;
}
```

**Helper method** (always include):
```csharp
private bool IsGuidNullOrEmpty(Guid? guid)
{
    return guid == null || guid == Guid.Empty;
}
```

#### Web API: `[ServiceName].WebApi`

- **Add controller**: `[Entities]Controller.cs` in `Controllers/` folder.
  - Attribute: `[ApiController]`
  - Route: `[Route("/v{version:ApiVersion}/[controller]")]`
  - Constructor receives: `I[Entity]Service`, `IMapper`, `ILogger<[Entities]Controller>` (and other required services).
  - All constructor parameters must have XML `<param>` comments.
  
  - **CRUD Endpoints** (HTTP verbs and routing):

    | Action | HTTP Method | Route | Parameter Source |
    |--------|-------------|-------|-----------------|
    | `AddAsync` | `[HttpPost]` | (default) | `[FromBody] Add[Entity]ViewModel` |
    | `UpdateAsync` | `[HttpPut("{id}")]` | `{id}` | `[FromRoute] Guid id`, `[FromBody] Update[Entity]ViewModel` |
    | `GetByIdAsync` | `[HttpGet("{id}")]` | `{id}` | `[FromRoute] Guid id` |
    | `GetAllAsync` | `[HttpPost("GetAll")]` | `GetAll` | `[FromBody] [Entities]Query` (or `GetAll[Entities]ViewModel`) |
    | `DeleteAsync` | `[HttpDelete("{id}")]` | `{id}` | `[FromRoute] Guid id` |

    > **Important**: The `GetAll` endpoint uses **`[HttpPost("GetAll")]`** (not `HttpGet`), because the filter/query object is sent in the request body.

  - **Authorization on each endpoint**:
    ```csharp
    #if EnableAuthorization
        [Authorize([ServiceName]Policies.CONSTANT_NAME)]
    #endif
    ```

  - **Response type attributes**:
    ```csharp
    [ProducesResponseType(typeof([Entity]ViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]  // for GetById, Update, Delete
    ```

  - **Use AutoMapper in every endpoint**:
    - `AddAsync`: Map `Add[Entity]ViewModel` → `[Entity]` (input), then map result `[Entity]` → `[Entity]ViewModel` (output).
    - `UpdateAsync`: Map `Update[Entity]ViewModel` properties onto the existing entity.
    - `GetByIdAsync`: Map `[Entity]` → `[Entity]ViewModel`.
    - `GetAllAsync`: Map `Page<[Entity]>` → `Page<[Entity]ViewModel>`.
    - Return responses **always as ViewModel**, never as Entity.

  - **Input Validation** (at the top of each endpoint, before calling services):
    - Validate required/non-null fields.
    - Return `BadRequest("descriptive message")` for invalid data.
    - Example:
      ```csharp
      if (IsGuidNullOrEmpty(id))
          return BadRequest("Invalid identifier.");
      ```

  - **Error Handling pattern** (wrap each endpoint body in try-catch):
    ```csharp
    try
    {
        _logger.LogInformation("[Entities]Controller: AddAsync - Adding [entity].");
        // ... validation ...
        // ... service call ...
        // ... map and return ...
    }
    catch (Exception ex)
    {
        _logger.LogError($"Error while adding [entity]: {ex}");
        return BadRequest(ex.Message);
    }
    ```

  - **Logging** (`ILogger<[Entities]Controller>`): 
    - Log at the **start** of every endpoint with `LogInformation`.
    - Log pattern: `"[Entities]Controller: [MethodName] - [Description]."`.
    - Log errors in catch blocks with `LogError`.

- **Add ViewModel classes** in `Models/[Entities]/` folder (plural name subfolder):
  - `Add[Entity]ViewModel.cs`: Input model for the Add endpoint.
  - `Update[Entity]ViewModel.cs`: Input model for the Update endpoint.
  - `[Entity]ViewModel.cs`: Output model returned in responses.
  - `GetAll[Entities]ViewModel.cs` *(optional)*: Input model for the GetAll endpoint if you want a WebApi-specific model instead of using the domain `[Entities]Query` directly.
  - All ViewModel properties must have XML documentation comments.

- **AutoMapper Profile** (`[ServiceName]AutoMapperProfile.cs` in WebApi root):
  - Must include:
    ```csharp
    // Generic Page mapping (required — always include this)
    CreateMap(typeof(Page<>), typeof(Page<>));
    
    // Entity mappings
    CreateMap<Add[Entity]ViewModel, [Entity]>();
    CreateMap<Update[Entity]ViewModel, [Entity]>();
    CreateMap<[Entity], [Entity]ViewModel>();
    
    // GetAll query mapping (if using a separate ViewModel for GetAll input)
    CreateMap<GetAll[Entities]ViewModel, [Entities]Query>();
    ```
  - **DateTime UTC handling**: When mapping DateTime properties from ViewModel to Query, use `SpecifyUtc` helper:
    ```csharp
    private static DateTime? SpecifyUtc(DateTime? dateTime)
    {
        return dateTime.HasValue ? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Utc) : (DateTime?)null;
    }
    ```

- **DI Registration** (in `Startup.cs` → `AddServices` method):
  - Services: `services.AddTransient<I[Entity]Service, [Entity]Service>();`
  - Repositories: `services.AddTransient<I[Entity]Repository, [Entity]Repository>();`
  - Use `AddTransient` for services and repositories.
  - Use `AddScoped` for filters (e.g., `DRFilterAttribute`, `ErrorFilterAttribute`).
  - Use `AddSingleton` for singleton services.

- **AutoMapper Registration** (in `Startup.cs` → `ConfigureServices` method):
  ```csharp
  services.AddAutoMapper([typeof([ServiceName]DomainAutoMapperProfile), typeof([ServiceName]AutoMapperProfile)]);
  ```

#### Test Projects

- In `[ServiceName].EF.Tests`:
  - Add `[Entity]RepositoryTests.cs` class.
  - Inherit from `[ServiceName]RepositoryTestsBase<Guid, [Entity], I[Entity]Repository>`.
  - Override `CreateDefaultEntity()` to return a valid entity instance.
  - Override `CreateRepository()` to create the repository with a test DbContext.
  - Override `ClearDataAsync()` to clean up test data.
  - Add the entity to `Common/TestEntityFactory.cs`.
  - Follow existing test structure — tests include: `AddAsync_NewEntity_AddsEntity`, `GetByIdAsync_ExistingEntity_ReturnsEntity`, `ChangeAsync_ExistingEntity_ChangesEntity`, `RemoveAsync_ExistingEntity_RemovesEntity`.

- In `[ServiceName].WebApi.IntegrationTests`:
  - Add `[Entity]ControllerTests.cs` class.
  - Inherit from `ControllerTestsBase`.
  - Implement tests: `CanAddEntityAsync`, `CanUpdateEntityAsync`, `CanGetAllEntitiesAsync`, `CanGetEntityByIdAsync`.
  - Add test factory methods to `Common/TestEntityFactory.cs`.
  - Use `FluentAssertions` for assertions (`.Should().Be(...)`, `.Should().NotBeNull()`, etc.).
  - Use `SendAsync` (POST), `PutAsync` (PUT), `HttpClient.GetAsync` (GET), `HttpClient.DeleteAsync` (DELETE) from the base class.

## Logging & Error Handling

### Logging:
  - We use `NLog` for logging across the application.
  - It will useful when you face issue in DEV/QA/SB environment.
  - Follow these guidelines for logging:
  - When to Use `_logger.LogInformation()`:
     - At the start of every endpoint.
     - Before and after calling services or performing critical operations.
     - Use descriptive and consistent messages.
     - Pattern: `"[Entities]Controller: [MethodName] - [Description]."`
     - Example: `_logger.LogInformation("ReportCategoriesController: Adding report category.");`

### Error Handling:
 - All controller endpoints must be wrapped in `try-catch` blocks.
 - Log errors using `_logger.LogError(...)` in the catch block.
 - Return `BadRequest(ex.Message)` in controllers (not `throw`).
 - In domain services, log and throw `DomainException`:
   ```csharp
   catch (Exception ex)
   {
       _logger.LogError(ex, "[Entity]Service: Error while [action].");
       throw new DomainException(
           $"Error while [action]: {ex.Message}",
           $"Error while [action]: {ex.Message}");
   }
   ```

## Development Workflow  (currently for developers done manually — will be automated in the future)

Follow this step-by-step process for working on any task.

- When start working on task then move the specific Task to **In Progress** state and first understand task and write what you understand in comment in task.
- **Create a branch from on-going-dev**.
  - **Branch Name format**: <Task#>-<short-description> (ex. : 12-add-organization)
- **Generate PR**:

  - When you complete your task then create Pull request from your branch into on-going-dev.
  - In your PR **select project**, **assign the PR to yourself** and **add required reviewers** in your PR for review and change state of PR to **In Progress**.
  - Move your task to **PR-pending review** in labels section of your task.
  - After PR approved then merge your PR into on-going-dev and **delete your branch**.
  - If you want to merge on-going-dev to main branch then create PR from on-going-dev into main and follow above steps to.

- **Deploy on DEV/QA/SB**:
  - Make a request in **Deployments Announcement & Discussion** for deploy on DEV/QA/SB
    - After Approve your request then deploy on DEV/QA/SB environment.
    - After deploy DEV/QA/SB send message in group with **Action : Done**
    - Then verify your task on DEV/QA/SB.
  - We have excel file for records deployment state of task. Make a entry in that excel file with Done status in your appropriate environment.
- Finally change your task status to **Done**.
