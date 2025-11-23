# Architecture Overview

## Project: Receipt Tracker - Personal Finance Application

**Version:** 1.0
**Last Updated:** 2025-11-23
**Status:** Phase 1.1 Complete

## Architecture Style

The Receipt Tracker application follows a **Layered Architecture** pattern with clear separation of concerns:

```
┌─────────────────────────────────────────────────────┐
│            Presentation Layer (UI)                  │
│         WPF with MVVM Pattern                       │
│         ReceiptTracker.UI                           │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│         Application/Service Layer                   │
│    Business Logic & Orchestration                   │
│         ReceiptTracker.Services                     │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│              Domain Layer                           │
│     Entities, Value Objects, Interfaces             │
│         ReceiptTracker.Core                         │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│         Infrastructure/Data Layer                   │
│   EF Core, Repositories, External Services          │
│         ReceiptTracker.Data                         │
└─────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Presentation Layer (ReceiptTracker.UI)
**Purpose:** User interface and user interaction logic

**Responsibilities:**
- XAML views and windows
- View models (MVVM pattern)
- UI-specific validation and formatting
- User input handling
- Data binding
- UI navigation

**Technologies:**
- WPF (Windows Presentation Foundation)
- Material Design In XAML
- MVVM pattern
- Dependency Injection (Microsoft.Extensions.DependencyInjection)

**Dependencies:**
- ReceiptTracker.Services
- ReceiptTracker.Core
- ReceiptTracker.Data

### 2. Application/Service Layer (ReceiptTracker.Services)
**Purpose:** Business logic and application orchestration

**Responsibilities:**
- Expense management business logic
- Receipt processing workflow
- OCR integration and coordination
- Analytics calculations
- Budget management
- Data import/export logic
- Business rule validation
- Application-level services

**Technologies:**
- .NET 8.0 class library
- Serilog for logging
- Dependency Injection

**Dependencies:**
- ReceiptTracker.Core
- ReceiptTracker.Data

### 3. Domain Layer (ReceiptTracker.Core)
**Purpose:** Core business domain and entities

**Responsibilities:**
- Domain entities (Receipt, Category, LineItem, etc.)
- Value objects (Money, DateRange, etc.)
- Domain interfaces (IRepository, IService)
- Domain enumerations
- Domain exceptions
- Business domain logic (no infrastructure concerns)

**Technologies:**
- .NET 8.0 class library
- No external dependencies (pure domain logic)

**Dependencies:**
- None (or only minimal abstractions like Microsoft.Extensions.Logging.Abstractions)

### 4. Infrastructure/Data Layer (ReceiptTracker.Data)
**Purpose:** Data persistence and external service integration

**Responsibilities:**
- Entity Framework Core DbContext
- Repository implementations
- Database migrations
- Data access logic
- External service integrations (OCR APIs, etc.)
- File system operations
- Caching

**Technologies:**
- Entity Framework Core 8.0
- SQLite database
- .NET 8.0 class library

**Dependencies:**
- ReceiptTracker.Core

## Design Patterns

### MVVM (Model-View-ViewModel)
Used in the presentation layer to separate UI from business logic:
- **Model:** Domain entities from Core layer
- **View:** XAML files
- **ViewModel:** View-specific logic and data binding

### Repository Pattern
Abstracts data access logic:
- Interfaces defined in Core layer
- Implementations in Data layer
- Provides testability and flexibility

### Unit of Work Pattern
Manages database transactions:
- Ensures data consistency
- Coordinates multiple repository operations
- Provides transaction boundaries

### Dependency Injection
Used throughout the application:
- Services registered at startup
- Constructor injection for dependencies
- Promotes loose coupling and testability

### Strategy Pattern
For pluggable components:
- OCR providers (Azure, Google, Tesseract, etc.)
- Export formats (CSV, Excel, PDF)
- Chart renderers

## Cross-Cutting Concerns

### Logging
- Serilog for structured logging
- Configured at application startup
- Available via dependency injection
- See: [Logging Strategy](LOGGING_STRATEGY.md)

### Error Handling
- Global exception handling in UI layer
- Specific exception types in domain layer
- Logging of all exceptions
- User-friendly error messages

### Validation
- Domain validation in entities
- Application validation in services
- UI validation in view models
- FluentValidation (to be added in later phases)

### Configuration
- appsettings.json for application settings
- User preferences stored in database
- Environment-specific configurations

## Data Flow Example: Receipt Processing

```
1. User uploads receipt image (UI Layer)
         ↓
2. UI calls ReceiptService.ProcessReceipt() (Service Layer)
         ↓
3. Service validates and saves image (Service Layer)
         ↓
4. Service calls OCR provider (Service Layer → Infrastructure)
         ↓
5. OCR extracts text (External Service)
         ↓
6. Service parses OCR results (Service Layer)
         ↓
7. Service creates Receipt entity (Domain Layer)
         ↓
8. Service saves to database via Repository (Data Layer)
         ↓
9. Repository uses EF Core to persist (Infrastructure)
         ↓
10. Success/failure returned to UI (All layers)
```

## Technology Decisions

For detailed rationale, see:
- [UI Framework Decision](UI_FRAMEWORK_DECISION.md) - Why WPF?
- [Logging Strategy](LOGGING_STRATEGY.md) - Why Serilog?

## Testing Strategy

### Unit Tests
- **Core Layer:** Pure business logic tests
- **Service Layer:** Service logic with mocked repositories
- **Data Layer:** Repository tests with in-memory database

### Integration Tests
- End-to-end receipt processing
- Database integration tests
- External service integration tests

### UI Tests
- Critical user journey tests (future phase)
- Accessibility tests

## Deployment Architecture

```
┌─────────────────────────────────────────────┐
│   Windows Desktop Application               │
│                                             │
│   ┌─────────────┐      ┌────────────┐      │
│   │  WPF UI     │      │  SQLite DB │      │
│   │  Process    │◄────►│   (Local)  │      │
│   └─────────────┘      └────────────┘      │
│         ↕                                   │
│   ┌─────────────┐                          │
│   │  Logs       │                          │
│   │  (Files)    │                          │
│   └─────────────┘                          │
└─────────────────────────────────────────────┘
         ↕ (Optional)
┌─────────────────────────────────────────────┐
│   Cloud OCR Services (Optional)             │
│   - Azure AI Document Intelligence          │
│   - Google Cloud Vision                     │
└─────────────────────────────────────────────┘
```

## Security Considerations

### Data Storage
- SQLite database stored locally
- Optional encryption at rest (SQLCipher - future phase)
- No cloud storage of sensitive data by default

### API Keys
- Stored in Windows Credential Manager
- Never committed to source control
- User-configurable

### Privacy
- All processing can be done locally
- Cloud OCR is optional
- No telemetry without user consent

## Performance Considerations

### Database
- Proper indexing on frequently queried columns
- Connection pooling
- Lazy loading for large datasets

### Image Processing
- Thumbnail generation for gallery views
- Background processing for OCR
- Image compression for storage

### UI
- Virtual scrolling for large lists
- Asynchronous operations
- Progress indicators for long-running tasks

## Future Enhancements

### Modularity
The architecture is designed to support future enhancements:
- Plugin system for OCR providers
- Export format plugins
- Custom category rule engines
- Cloud sync modules

### Scalability
While designed as a desktop application, the layered architecture allows:
- Migration to cloud-based storage
- Web API extraction for multi-platform support
- Mobile app integration

## References

- [Microsoft WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)

---

**Next Phase:** Phase 1.2 - Detailed Architecture Design
