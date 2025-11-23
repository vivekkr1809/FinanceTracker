# Layered Architecture Boundaries

## Receipt Tracker - Layer Definitions and Contracts

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## Overview

The Receipt Tracker application follows a strict layered architecture with clear boundaries and responsibilities. This document defines what belongs in each layer, what dependencies are allowed, and what contracts exist between layers.

---

## Dependency Rules

### Core Principle: Dependencies Point Inward

```
UI Layer → Services Layer → Domain Layer ← Data Layer
   │            │              ▲            │
   │            │              │            │
   │            └──────────────┴────────────┘
   │                   Both depend on
   └────────────────→  Domain abstractions
```

### Allowed Dependencies

| Layer | Can Reference | Cannot Reference |
|-------|--------------|------------------|
| **Domain** | None (Pure C#) | Services, Data, UI |
| **Services** | Domain | Data, UI |
| **Data** | Domain | Services, UI |
| **UI** | Services, Domain, Data | None (top layer) |

---

## Layer 1: Domain Layer (ReceiptTracker.Core)

### Purpose
Contains the core business domain model and business logic that is independent of any infrastructure, UI, or external concerns.

### Responsibilities

#### ✅ What Belongs Here

1. **Domain Entities**
   - Receipt, LineItem, Category, Tag, Budget
   - PaymentMethod, ExpenseBudget, etc.
   - Entity base classes with identity

2. **Value Objects**
   - Money (amount + currency)
   - DateRange (start date, end date)
   - Address
   - Percentage
   - ImageMetadata

3. **Domain Enumerations**
   - CategoryType
   - PaymentMethodType
   - BudgetPeriodType
   - ReceiptStatus
   - OcrConfidenceLevel

4. **Domain Interfaces**
   - IRepository<TEntity>
   - IUnitOfWork
   - IOcrProvider
   - IImageProcessor
   - IExportService

5. **Domain Exceptions**
   - DomainException (base)
   - EntityNotFoundException
   - ValidationException
   - BusinessRuleViolationException
   - DuplicateEntityException

6. **Domain Events** (Future)
   - ReceiptCreatedEvent
   - BudgetExceededEvent
   - CategoryChangedEvent

7. **Specifications/Business Rules**
   - Receipt validation rules
   - Budget calculation rules
   - Category assignment rules

#### ❌ What Does NOT Belong Here

- Database context or EF Core entities
- HTTP clients or API calls
- File I/O operations
- UI-specific logic
- External service integrations
- Dependency on any framework except basic .NET

### Dependencies
- ✅ None (or only Microsoft.Extensions.Logging.Abstractions for logging interfaces)
- ✅ System namespaces only

### Example Structure

```
ReceiptTracker.Core/
├── Domain/
│   ├── Entities/
│   │   ├── Base/
│   │   │   ├── Entity.cs
│   │   │   └── AggregateRoot.cs
│   │   ├── Receipt.cs
│   │   ├── LineItem.cs
│   │   ├── Category.cs
│   │   ├── Tag.cs
│   │   ├── PaymentMethod.cs
│   │   └── ExpenseBudget.cs
│   ├── ValueObjects/
│   │   ├── Money.cs
│   │   ├── DateRange.cs
│   │   ├── Address.cs
│   │   └── ImageMetadata.cs
│   ├── Enums/
│   │   ├── CategoryType.cs
│   │   ├── PaymentMethodType.cs
│   │   └── BudgetPeriodType.cs
│   └── Specifications/
│       └── ReceiptSpecifications.cs
├── Interfaces/
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── IReceiptRepository.cs
│   │   ├── ICategoryRepository.cs
│   │   └── IUnitOfWork.cs
│   ├── Services/
│   │   ├── IOcrProvider.cs
│   │   ├── IImageProcessor.cs
│   │   └── IExportService.cs
│   └── Infrastructure/
│       └── IDateTime.cs (abstraction for DateTime.Now)
└── Exceptions/
    ├── DomainException.cs
    ├── EntityNotFoundException.cs
    ├── ValidationException.cs
    └── BusinessRuleViolationException.cs
```

### Contracts

**Entities must:**
- Have a unique identifier (Id property)
- Implement proper equality comparison
- Contain domain validation logic
- Be persistence-ignorant (no EF attributes)

**Value Objects must:**
- Be immutable
- Implement value-based equality
- Contain validation in constructor

**Interfaces must:**
- Define contracts without implementation
- Use domain types only (no DTOs)
- Be technology-agnostic

---

## Layer 2: Application/Service Layer (ReceiptTracker.Services)

### Purpose
Orchestrates business workflows, coordinates domain entities, and implements use cases. Acts as the boundary between the domain and external concerns.

### Responsibilities

#### ✅ What Belongs Here

1. **Application Services**
   - ExpenseService
   - ReceiptService
   - CategoryService
   - BudgetService
   - AnalyticsService
   - ReportService

2. **Business Workflow Orchestration**
   - Receipt processing pipeline
   - Budget calculation workflows
   - Data import/export processes

3. **Service Interfaces and DTOs**
   - Request/Response DTOs
   - Service interfaces
   - Command/Query objects (CQRS pattern if used)

4. **Business Logic Validation**
   - Cross-entity validation
   - Complex business rules
   - Transaction coordination

5. **External Service Integration**
   - OCR provider implementations
   - Email service integration
   - Export format generators

6. **Mapping/Transformation**
   - Entity to DTO mapping
   - DTO to Entity mapping

#### ❌ What Does NOT Belong Here

- Database context implementation (belongs in Data layer)
- Direct SQL queries
- UI-specific logic
- EF Core migrations
- Direct file system access (use interfaces from Domain)

### Dependencies
- ✅ ReceiptTracker.Core (Domain)
- ✅ ReceiptTracker.Data (for registration only, not business logic)
- ✅ Microsoft.Extensions.Logging
- ✅ Third-party libraries (AutoMapper, etc.)

### Example Structure

```
ReceiptTracker.Services/
├── Expenses/
│   ├── ExpenseService.cs
│   ├── IExpenseService.cs
│   └── DTOs/
│       ├── CreateExpenseRequest.cs
│       ├── UpdateExpenseRequest.cs
│       └── ExpenseResponse.cs
├── Receipts/
│   ├── ReceiptService.cs
│   ├── IReceiptService.cs
│   ├── ReceiptProcessingPipeline.cs
│   └── DTOs/
│       ├── ReceiptUploadRequest.cs
│       └── ReceiptDetailResponse.cs
├── OCR/
│   ├── IOcrService.cs
│   ├── Providers/
│   │   ├── AzureOcrProvider.cs
│   │   ├── TesseractOcrProvider.cs
│   │   └── WindowsOcrProvider.cs
│   └── OcrResultParser.cs
├── Analytics/
│   ├── AnalyticsService.cs
│   ├── IAnalyticsService.cs
│   └── DTOs/
│       └── SpendingAnalyticsResponse.cs
├── Budgets/
│   ├── BudgetService.cs
│   ├── IBudgetService.cs
│   └── BudgetCalculator.cs
├── Export/
│   ├── IExportService.cs
│   ├── CsvExportService.cs
│   ├── ExcelExportService.cs
│   └── PdfExportService.cs
├── Import/
│   ├── IImportService.cs
│   └── CsvImportService.cs
├── Mapping/
│   └── AutoMapperProfile.cs
└── Logging/
    └── LoggingConfiguration.cs
```

### Contracts

**Services must:**
- Accept DTOs or primitives as parameters (not entities directly from UI)
- Return DTOs or primitives (not entities)
- Be stateless
- Use constructor dependency injection
- Log all operations
- Handle and translate domain exceptions to application exceptions

**Service Methods should:**
- Have clear, intention-revealing names (ProcessReceipt, CalculateBudget)
- Be async when performing I/O
- Use Unit of Work for transactions
- Validate input before passing to domain

---

## Layer 3: Data/Infrastructure Layer (ReceiptTracker.Data)

### Purpose
Implements data persistence, external service integrations, and infrastructure concerns. Provides concrete implementations of interfaces defined in the Domain layer.

### Responsibilities

#### ✅ What Belongs Here

1. **Database Context**
   - EF Core DbContext
   - Entity configurations
   - Database migrations

2. **Repository Implementations**
   - ReceiptRepository
   - CategoryRepository
   - BudgetRepository
   - Generic Repository<T>

3. **Unit of Work Implementation**
   - Transaction management
   - SaveChanges coordination

4. **Entity Framework Configurations**
   - Fluent API configurations
   - Table mappings
   - Relationships
   - Indexes

5. **Data Access Utilities**
   - Query extensions
   - Specification pattern implementations

6. **Database Seeding**
   - Default categories
   - Initial data

7. **File System Access**
   - Image storage
   - Backup creation

8. **External Service Clients** (if not in Services)
   - API clients for cloud services

#### ❌ What Does NOT Belong Here

- Business logic
- UI logic
- Domain entity definitions (they're in Core)
- Service orchestration

### Dependencies
- ✅ ReceiptTracker.Core (Domain)
- ✅ Entity Framework Core
- ✅ SQLite provider
- ✅ Microsoft.Extensions.Logging

### Example Structure

```
ReceiptTracker.Data/
├── Context/
│   ├── ReceiptTrackerDbContext.cs
│   └── DbContextFactory.cs (for design-time)
├── Configurations/
│   ├── ReceiptConfiguration.cs
│   ├── CategoryConfiguration.cs
│   ├── LineItemConfiguration.cs
│   └── BudgetConfiguration.cs
├── Repositories/
│   ├── Repository.cs (generic base)
│   ├── ReceiptRepository.cs
│   ├── CategoryRepository.cs
│   ├── BudgetRepository.cs
│   └── UnitOfWork.cs
├── Migrations/
│   └── (EF Core generated migrations)
├── Seeding/
│   └── DataSeeder.cs
└── Extensions/
    └── QueryExtensions.cs
```

### Contracts

**DbContext must:**
- Configure all entities using Fluent API
- Use conventions consistently
- Handle concurrency properly
- Implement soft delete if required

**Repositories must:**
- Implement interface from Domain layer
- Use async operations
- Handle exceptions appropriately
- Not expose IQueryable outside the layer (return materialized results)
- Use specifications for complex queries

---

## Layer 4: Presentation Layer (ReceiptTracker.UI)

### Purpose
Handles all user interface concerns, user interactions, and presentation logic. Implements MVVM pattern for separation of UI and logic.

### Responsibilities

#### ✅ What Belongs Here

1. **Views (XAML)**
   - MainWindow
   - DashboardView
   - ReceiptListView
   - ReceiptDetailView
   - AnalyticsView
   - SettingsView

2. **ViewModels**
   - MainViewModel
   - DashboardViewModel
   - ReceiptListViewModel
   - ReceiptDetailViewModel
   - AnalyticsViewModel

3. **UI-Specific Services**
   - NavigationService
   - DialogService
   - NotificationService

4. **Commands**
   - RelayCommand / DelegateCommand implementations
   - Command handlers

5. **Converters**
   - Value converters for data binding
   - Multi-value converters

6. **Behaviors**
   - Attached behaviors
   - Triggers

7. **Validators**
   - Input validation rules for UI
   - Display-only validation (real validation in domain)

8. **Resources**
   - Styles
   - Templates
   - Images/Icons

#### ❌ What Does NOT Belong Here

- Database access
- Business logic
- Data persistence
- OCR processing
- Complex calculations

### Dependencies
- ✅ ReceiptTracker.Services
- ✅ ReceiptTracker.Core (for entities/DTOs)
- ✅ ReceiptTracker.Data (only for DI registration)
- ✅ WPF framework
- ✅ Material Design In XAML
- ✅ MVVM framework (if using one like Prism or MVVM Light)

### Example Structure

```
ReceiptTracker.UI/
├── Views/
│   ├── MainWindow.xaml
│   ├── Dashboard/
│   │   └── DashboardView.xaml
│   ├── Receipts/
│   │   ├── ReceiptListView.xaml
│   │   └── ReceiptDetailView.xaml
│   ├── Analytics/
│   │   └── AnalyticsView.xaml
│   └── Settings/
│       └── SettingsView.xaml
├── ViewModels/
│   ├── Base/
│   │   ├── ViewModelBase.cs
│   │   └── IViewModel.cs
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── ReceiptListViewModel.cs
│   ├── ReceiptDetailViewModel.cs
│   └── AnalyticsViewModel.cs
├── Commands/
│   ├── RelayCommand.cs
│   └── AsyncRelayCommand.cs
├── Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── DateToStringConverter.cs
│   └── MoneyToStringConverter.cs
├── Behaviors/
│   └── DragDropBehavior.cs
├── Services/
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDialogService.cs
│   └── DialogService.cs
├── Resources/
│   ├── Styles/
│   ├── Templates/
│   └── Images/
└── Bootstrapper/
    └── DependencyInjectionConfig.cs
```

### Contracts

**ViewModels must:**
- Implement INotifyPropertyChanged
- Not hold references to Views
- Use Commands for user actions
- Be testable (no UI dependencies)
- Use services via interfaces
- Handle async operations properly

**Views must:**
- Contain no code-behind logic (except DI wiring)
- Use data binding exclusively
- Not reference ViewModels directly in XAML (use DataContext)

---

## Cross-Layer Contracts

### Data Transfer Between Layers

```
UI Layer        ← DTOs →      Service Layer    ← Entities →   Domain Layer
                                    ↓
                              Repository Interface
                                    ↓
                              Data Layer       ← Entities →   Domain Layer
```

### Communication Flow

1. **UI → Services**
   - Use: DTOs, primitives, commands
   - Return: DTOs, view models
   - Never: Domain entities directly

2. **Services → Domain**
   - Use: Domain entities, value objects
   - Return: Domain entities
   - Translate: DTOs to/from entities

3. **Services → Data**
   - Use: Repository interfaces (from Domain)
   - Provide: Entities to persist
   - Receive: Entities from queries

4. **Data → Database**
   - Use: EF Core entities (same as domain entities)
   - Handle: Mapping, tracking, transactions

---

## Exception Flow

```
Layer           Throws                          Catches                     Transforms To
-----------------------------------------------------------------------------------------------
Domain          DomainException                 -                           -
                ValidationException

Services        ApplicationException            DomainException             ApplicationException
                NotFoundException

Data            DataException                   DbUpdateException           DataException
                                                SqliteException

UI              -                               ApplicationException        User message
                                                All exceptions              Error dialog
```

---

## Logging Strategy by Layer

| Layer | What to Log | Log Level |
|-------|-------------|-----------|
| **Domain** | Business rule violations | Warning/Error |
| **Services** | Service calls, workflow steps | Information |
| **Services** | Exceptions | Error |
| **Data** | Database operations | Debug |
| **Data** | Connection issues | Error |
| **UI** | User actions | Information |
| **UI** | Unhandled exceptions | Critical |

---

## Testing Strategy by Layer

| Layer | Test Type | Focus |
|-------|-----------|-------|
| **Domain** | Unit Tests | Business logic, validation |
| **Services** | Unit Tests | Service logic with mocked repos |
| **Services** | Integration Tests | With real database (in-memory) |
| **Data** | Integration Tests | Repository implementations |
| **UI** | Unit Tests | ViewModel logic |
| **UI** | UI Tests (Optional) | User workflows |

---

## Key Principles

### 1. Dependency Inversion
- High-level modules (Services) don't depend on low-level modules (Data)
- Both depend on abstractions (interfaces in Domain)

### 2. Interface Segregation
- Small, focused interfaces
- One interface per repository type
- Service interfaces specific to use cases

### 3. Single Responsibility
- Each layer has one reason to change
- Domain: Business rules change
- Services: Workflows change
- Data: Storage mechanism changes
- UI: User interface changes

### 4. Open/Closed Principle
- Layers open for extension (new services, new repositories)
- Closed for modification (stable interfaces)

---

## Anti-Patterns to Avoid

### ❌ Layer Violations

**Don't:**
- Access DbContext from ViewModels
- Put business logic in ViewModels
- Put UI logic in Services
- Reference Data layer from ViewModels (except for DI setup)

**Do:**
- Use services as the only entry point from UI
- Keep ViewModels thin (delegate to services)
- Keep business logic in Domain and Services

### ❌ Leaky Abstractions

**Don't:**
- Return IQueryable from repositories
- Expose EF entities to UI
- Use database-specific types in Domain

**Do:**
- Return materialized collections
- Use DTOs for UI communication
- Use framework-agnostic types in Domain

### ❌ God Objects

**Don't:**
- Create single service with all methods
- Create single repository for all entities

**Do:**
- Create focused services per feature area
- Create repository per aggregate root

---

## Validation Strategy

### Where Validation Happens

1. **UI Layer (ViewModels)**
   - Input format validation
   - Required field validation
   - Display-only (real validation in domain)

2. **Domain Layer**
   - Business rule validation
   - Entity invariants
   - Value object validation

3. **Service Layer**
   - Cross-entity validation
   - Workflow validation
   - Permission checks (future)

---

**Next Steps:**
- Implement base classes for each layer
- Create sample entity and repository
- Implement DI configuration
- Create coding standards per layer
