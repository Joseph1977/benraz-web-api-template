# Project Development Guide

## Purpose of the Document

The purpose of this document is to serve as a comprehensive guide for developers and contributors working on the project. It outlines the project's structure, best practices, coding standards, development workflow, and architectural decisions to ensure consistency, maintainability, and scalability across the entire codebase.

---

## Project Structure Overview

This service follows a layered microservice architecture with a clear separation of concerns across multiple projects:

### [ServiceName].WebApi – Main ASP.NET Core Web API project
- The main entry point of the microservice and Responsibilities include:
  - **HTTP Endpoints (Controllers)**: Expose RESTful APIs.
  - **Request/Response Mapping**:
    - Uses AutoMapper to convert entity models to view models and vice versa.
    - Entity models are never returned directly in responses.
  - **Swagger Integration**:
    - Web UI tool to view, test, and interact with API endpoints. You can use like `{domain}/swagger` in browser.
  - **Logging**:
    - Integrated NLog for structured logging.
    - Use for information log and error log.
    - `nlog.config` file contains configuration for NLog and is located in the WebApi project.
  - **Authorization**:
    - Uses Claim-based policies via `[Authorize(POLICYNAME)]` attribute.
    - **Claim format**: `XXX_SERVICE_XXX`
    - **Policy format**: `[ControllerName-Action]`
    - To add a new endpoint:
      - Add the claim in `Claims` class (inside Domain/Authorization).
      - Add the policy in `Policies` class (inside WebApi/Authorization).
      - Register policy in `Startup.cs`:  
        `options.AddClaimsPolicy("PolicyName", "ClaimName");`
      - Update claims script and Excel sheet (in `/docs` folder).
  - **Authentication Toggle**:
    - Uses `#ifdef` to disable authentication during local debugging when no auth server is available.
    - Always build in **Release mode** before committing code.
  - **Environment Configurations**:
      - See `Environment Configurations (Mix DB Vendors)` section.

  - **Special Controllers**:
    - `JobsController`: 
        - Exposes API endpoints to run background tasks triggered by an external scheduler.
        - Includes a sample Execute endpoint for reference.

    - `ITController`: 
        - Includes an IsAlive endpoint that returns HTTP 200 (OK). 
        - This helps to easily check if the microservice is running and healthy.
  - **Documentation**:
    - Every method/property must have XML summary and param comments ending with a period (`.`).

### [ServiceName].Domain – Domain Layer
- Contains core business logic:
  - Domain models
  - Interfaces
  - Domain services
  - Repository interfaces
  - Business logic

### [ServiceName].EF – Data Access Layer (Entity Framework Core)
- Responsible for:
 - Implements repositories from the Domain layer
 - Database operations
 - `DbContext` and configurations
 - Entity configurations
 - Seed data
 - Migration services

### [ServiceName].Domain.Tests – Unit Tests for Domain
- Tests business logic and domain services for the Domain layer.

### [ServiceName].EF.Tests – Unit Tests for EF Layer
  - Validates entity configurations, repository logic, and EF Core operations.
  - Create unit test case for entity like (add, update, delete, get, get all) using existing structure.

### [ServiceName].WebApi.IntegrationTests – Integration Tests for Web API
- Crete unit test case for controller endpoints like (add, update, delete, get, get all) using existing structure.

This template provides three different implementation approaches to help you create a microservice that best fits your deployment and configuration requirements.

## Environment Configurations (`Mix DB Vendors`)

### 1. **Environment Variables Implementation** (`Mix DB Vendors`)

**Best for:** Applications that need to support multiple database providers (`SQL Server` and `PostgreSQL`).

**Features:**
- Configuration via environment variables (no `appsettings.json` required)
- Support for both `SQL Server` AND `PostgreSQL`
- Separate Entity Framework Core projects per database provider to isolate migrations and configurations like:
  - `[ServiceName].EF.PostgreSql` : For PostgreSql
  - `[ServiceName].EF.SqlServer` : For SQL Server
- Uses preprocessor directives (`#if`, `#elif`) to compile conditionally for the target database
- Use Shared data access layer `[ServiceName].EF.Data`
- IF `SQLSERVER` environment variable `true` then use `SQL Server` other wise use `PostgreSQL`.
- Use `DesignTimeDbContextFactory` class for local migration.
- All implementations support the `SkipDbConnectIfNoConnectionString` option:
```properties
SkipDbConnectIfNoConnectionString=true
```
When enabled, the service will start without database connectivity if no connection string is provided.

**When to use:**
- Use this implementation when:
  - You're building multi-tenant applications where tenants use different database engines
  - Applications that need database vendor flexibility at build or deploy time
  - When client requirements vary by deployment
  
---


## Folder Structure

Use existing projects like:

- `MyService.Domain`
- `MyService.WebApi`
- `MyService.EF`
- `MyService.WebApi.IntegrationTests`

> **Do NOT** create `[ServiceName]` folders. Always use the actual project name.

## Our NuGet Library: 

## **`tn-infra-nugets`**
   -Shared packages across all services:
   - Repository: https://github.com/TomNextDev/tn-infra-nugets
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
              `tn-infra-nugets/TomNext.Infrastructure.Gateways/TomNext[ServiceName]` for example (e.g., `TomNextAssets/`)
           - Folder Structure (inside `TomNext[ServiceName]`):
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
           - ITargetServiceGateway.cs`:
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
   ### Benraz.Infrastructure.Common: 
     - Provides common infrastructure abstractions and base types for access control and entity management, supporting both web and domain layers.
   ### Benraz.Infrastructure.Domain:
     - Domain layer containing common domain entities
   ### Benraz.Infrastructure.Gateways:
     - Internal and 3rd-party gateway implementations
   ### Benraz.Infrastructure.Authorization:
     - Authorization services and JWT token management
   ### Benraz.Infrastructure.Files: 
     - Cross-platform file management services (Azure Blob Storage support, Google Cloud Storage integration)
   ### Benraz.Infrastructure.Phone:
     - Phone/SMS services and management

   - Repository: https://github.com/Joseph1977/benraz-infra-nugets

## Technical Constraints / Limitations
- To ensure consistency, maintainability, and adherence to architectural principles, the following technical constraints apply:
- **Framework & Language**:
  - Target Framework: .NET 8
  - Language Version: C# 12
- **Architecture Rules**:
  - **No direct database access in WebAPI**
    - All data access must go through the domain layer.
  - **No domain logic in controllers**
    - Controllers must delegate all processing to application/services layer.
- **Library Usage**:
  - Use only approved libraries, including:
    - EF Core for data access
    - AutoMapper for object mapping
  - Any third-party libraries must be approved before use.

## Naming Conventions

| Element         | Convention                                 |
|---------------- |--------------------------------------------|
| Files/Classes   | PascalCase                                 |
| Interfaces      | Prefix with `I` (e.g., ISettingsService)   |
| Methods         | PascalCase      (e.g., GetAllSettings)     |
| Variables       | camelCase       (e.g., settingViewModel)   |
| Properties      | PascalCase      (e.g., Name)               |
| Folders         | PascalCase      (e.g., Assets)             |

- Braces `{}` **always on a new line**
- Use `var` **only if type is obvious**
- Every method/property must have XML summary and param comments ending with a period (`.`).

## Async/Await Conventions

- All async methods must have `Async` suffix
- Use `Task<T>` instead of `void`
- Always use `await` keyword

**✅ Correct:**
```public async Task<User> GetUserAsync(Guid userId)
{
    return await _userService.GetByIdAsync(userId);
}
```

**❌ Incorrect:**

```
public async Task GetUser(Guid userId)
{
    _userService.GetByIdAsync(userId); // No await!
}
```

## Development Rules / Guidelines

### Adding New Features

- Add endpoint in WebApi controller
- Define entity/interface/service in Domain layer
- Implement service/repository in EF layer
- Wire up with DI
- Use AutoMapper to map domain ↔ DTO
- Add Authorization policy & claims
- Add tests in test projects

### Copilot Task Template – Add New Entity (e.g., Settings)

#### Domain: `[ServiceName].Domain`
- Add folder : `Settings`
- Files:
  - `Setting.cs`: Entity model
     - Must inherit from the `AggregateRootBase<Guid>` class.
     - `AggregateRootBase` includes: `Id`, `CreateTimeUtc`, `UpdateTimeUtc` properties.

  - `ISettingService.cs`: Interface for setting service
  - `SettingService.cs`: Implements interface
  - `ISettingRepository.cs`: Repository interface
     - Must inherit from `IRepository<Guid, EntityModel>`
  - Optional: Filter Support for GetAll Endpoints
    - To support advanced filtering, sorting, and pagination in GetAll endpoints, implement the following:
      - Create the following classes:
        - `SettingQuery.cs`: 
        - This class represents the query model for filtering. It should:
        - Include all entity properties as nullable to support flexible filtering.
        - Additionally include the following standard properties:
             - `public int PageNo { get; set; }`
	     - `public int PageSize { get; set; }`
	     - `public bool Any { get; set; }`
	     - `public SettingQueryParameters? SortBy { get; set; }`
             - `public bool SortDesc { get; set; }`
	     - `public DateTime? CreateTimeUtcRangeFrom { get; set; }`
	     - `public DateTime? CreateTimeUtcRangeTo { get; set; }`
	     - `public DateTime? UpdateTimeUtcRangeFrom { get; set; }`
	     - `public DateTime? UpdateTimeUtcRangeTo { get; set; }`
 
            - `SettingQueryParameters.cs`:
               - Define an enum that includes all sortable entity properties.
               - Example:
		```csharp
		 public enum SettingQueryParameters
		 {
		    Name = 1,
		    // Add more properties as needed
		 }
		```
   
#### EF Layer: `[ServiceName].EF`
- Folder: `Repositories`
  - Add `SettingRepository.cs`
  - Implementation details:
    ```public class SettingRepository : BaseRepository<Setting>, ISettingRepository
    {
      private readonly DBContextClass _context;
      public SettingRepository(DBContextClass context) : base(context)
      {
        _context = context;
      }
      // Add repository methods here, using _context if needed
    }

- Folder: `Configurations`
  - Add `SettingConfiguration.cs`
  - Must implement the EF Core configuration interface:
    - `SettingConfiguration.cs`: Inherits `IEntityTypeConfiguration<Setting>`

    ```public class SettingConfiguration : IEntityTypeConfiguration<Setting>
    {
        public void Configure(EntityTypeBuilder<Setting> builder)
        {
            // Configure entity properties, keys, indexes, constraints here
        }
    }
 - Note: Replace DBContextClass with actual DbContext name.

#### Web API: `[ServiceName].WebApi`
- Add controller: `SettingsController.cs`
  - Add Route: `[Route("/v{version:ApiVersion}/[controller]")]`
  - CRUD Endpoints
    - Implement the following endpoints using the domain service layer:
      - `AddAsync`
      - `UpdateAsync`
      - `GetByIdAsync`
      - `GetAllAsync` (with optional filtering/paging)
      - `DeleteAsync`
  - Use AutoMapper:
    - Map between Entity and ViewModel both ways and return response as a view model.
    - Return responses as ViewModel, not Entity.
 - Input Validation
   - Validate required/non-null fields.
   - Return `BadRequest(...)` for invalid data:
   - Example:
      ```if (string.IsNullOrEmpty(id))
         return BadRequest("Invalid settings identifier");
 - Logging (`ILogger`): 
   - Use `LogInformation` method of logger in endpoint for log information.
   - Use `LogError` method for log error.
   - Use `ILogger` to add meaningful logs in each controller action for:
     - Start of the method (before calling any service)
     - Any errors (inside catch blocks)

   - Example Logging Pattern:
```csharp
     _logger.LogInformation("SettingsController: AddAsync - Start");
    try {
        ...
    } catch (Exception ex) {
        _logger.LogError(ex, "SettingsController: Error in AddAsync");
        throw;
    }
```
    
 - Add Authorization:
   - Update Claims and Policies as per earlier instructions


#### Test Projects
Test Projects:
- In `[ServiceName].EF.Tests`:
     -  Add `SettingRepositoryTests.cs` class and implement all unit test case for entity. Follow existing structure.
- In `[ServiceName].WebApi.IntegrationTests`, add (if needed):
     - Add `SettingControllerTests.cs` class and implement all unit test case for controller. Follow existing structure.

## Logging & Error Handling

### Logging:
  - We use `NLog` for logging across the application.
  - It will useful when you face issue in DEV/QA/SB environment.
  - Follow these guidelines for logging:
  -  When to Use `_logger.LogInformation()`:
     - At the start of every endpoint.
     - Before and after calling services or performing critical operations.
     - Use descriptive and consistent messages.
     - Example : `_logger.LogInformation("SettingsController: AddAsync");`

### Error Handling:
 - Use `try-catch` blocks to handle exceptions gracefully.
 - All controller endpoints must include proper exception handling.
 - Log errors using `_logger.LogError(...)` in the catch block.
 - Never suppress exceptions silently—log and rethrow or handle appropriately.
 - Every catch block add error log like below:
   ```catch (Exception ex)
    {
        _logger.LogError(ex, "SettingsController: Error while adding settings.");
         throw;
    }

## Development Workflow

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
