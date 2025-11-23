# High-Level Architecture Diagram

## Receipt Tracker - System Architecture

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## System Context Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      Windows Desktop User                       │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                   Receipt Tracker Application                   │
│                        (WPF .NET 8)                             │
└───────┬─────────────────────────────────┬───────────────────────┘
        │                                 │
        ▼                                 ▼
┌───────────────┐                 ┌──────────────────┐
│  File System  │                 │  Cloud OCR API   │
│  (Images, DB) │                 │   (Optional)     │
└───────────────┘                 └──────────────────┘
```

---

## Layered Architecture Diagram

```
╔═══════════════════════════════════════════════════════════════════╗
║                      PRESENTATION LAYER                           ║
║                   (ReceiptTracker.UI)                             ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐           ║
║  │    Views     │  │  ViewModels  │  │   Behaviors  │           ║
║  │   (XAML)     │  │   (MVVM)     │  │  Converters  │           ║
║  └──────────────┘  └──────────────┘  └──────────────┘           ║
║                                                                   ║
║  ┌──────────────────────────────────────────────────┐            ║
║  │         Dependency Injection Container           │            ║
║  └──────────────────────────────────────────────────┘            ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
                              │
                              ▼
╔═══════════════════════════════════════════════════════════════════╗
║                    APPLICATION/SERVICE LAYER                      ║
║                  (ReceiptTracker.Services)                        ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              ║
║  │  Expense    │  │   Receipt   │  │  Analytics  │              ║
║  │  Service    │  │   Service   │  │   Service   │              ║
║  └─────────────┘  └─────────────┘  └─────────────┘              ║
║                                                                   ║
║  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              ║
║  │    OCR      │  │   Budget    │  │   Export    │              ║
║  │  Service    │  │   Service   │  │   Service   │              ║
║  └─────────────┘  └─────────────┘  └─────────────┘              ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
                              │
                              ▼
╔═══════════════════════════════════════════════════════════════════╗
║                         DOMAIN LAYER                              ║
║                    (ReceiptTracker.Core)                          ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  ┌────────────────────────────────────────────────────┐           ║
║  │              Domain Entities                       │           ║
║  │  Receipt, LineItem, Category, Tag, Budget, etc.   │           ║
║  └────────────────────────────────────────────────────┘           ║
║                                                                   ║
║  ┌────────────────────────────────────────────────────┐           ║
║  │            Value Objects                           │           ║
║  │  Money, DateRange, Address, etc.                  │           ║
║  └────────────────────────────────────────────────────┘           ║
║                                                                   ║
║  ┌────────────────────────────────────────────────────┐           ║
║  │         Domain Interfaces & Contracts              │           ║
║  │  IRepository<T>, IUnitOfWork, IOcrProvider, etc.  │           ║
║  └────────────────────────────────────────────────────┘           ║
║                                                                   ║
║  ┌────────────────────────────────────────────────────┐           ║
║  │           Domain Exceptions                        │           ║
║  │  DomainException, ValidationException, etc.       │           ║
║  └────────────────────────────────────────────────────┘           ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
                              │
                              ▼
╔═══════════════════════════════════════════════════════════════════╗
║                    INFRASTRUCTURE LAYER                           ║
║                   (ReceiptTracker.Data)                           ║
╠═══════════════════════════════════════════════════════════════════╣
║                                                                   ║
║  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              ║
║  │   EF Core   │  │ Repository  │  │  Unit of    │              ║
║  │  DbContext  │  │Implementa-  │  │    Work     │              ║
║  │             │  │   tions     │  │             │              ║
║  └─────────────┘  └─────────────┘  └─────────────┘              ║
║                                                                   ║
║  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              ║
║  │  Database   │  │     OCR     │  │    File     │              ║
║  │ Migrations  │  │  Providers  │  │   Storage   │              ║
║  └─────────────┘  └─────────────┘  └─────────────┘              ║
║                                                                   ║
╚═══════════════════════════════════════════════════════════════════╝
                              │
                              ▼
                      ┌───────────────┐
                      │ SQLite        │
                      │ Database      │
                      └───────────────┘
```

---

## Component Interaction Diagram

### Receipt Upload and Processing Flow

```
User
  │
  │ 1. Upload Image
  ▼
┌─────────────────┐
│  MainWindow     │
│  (View)         │
└────────┬────────┘
         │
         │ 2. Execute Command
         ▼
┌─────────────────┐
│  ReceiptViewModel│
└────────┬────────┘
         │
         │ 3. Call Service
         ▼
┌─────────────────┐
│ ReceiptService  │
│  (Application)  │
└────────┬────────┘
         │
         ├─────── 4. Save Image ────────┐
         │                              ▼
         │                      ┌──────────────┐
         │                      │FileStorage   │
         │                      │Service       │
         │                      └──────────────┘
         │
         ├─────── 5. Process OCR ───────┐
         │                              ▼
         │                      ┌──────────────┐
         │                      │ IOcrProvider │
         │                      │ (Interface)  │
         │                      └──────┬───────┘
         │                             │
         │                   ┌─────────┴─────────┐
         │                   ▼                   ▼
         │            ┌──────────┐      ┌──────────────┐
         │            │ Azure    │      │  Tesseract   │
         │            │ OCR      │      │  (Offline)   │
         │            └──────────┘      └──────────────┘
         │
         │ 6. Parse Results
         │
         ├─────── 7. Create Entity ─────┐
         │                              ▼
         │                      ┌──────────────┐
         │                      │   Receipt    │
         │                      │   (Entity)   │
         │                      └──────────────┘
         │
         │ 8. Save to Repository
         ▼
┌─────────────────┐
│ IReceiptRepo    │
│  (Interface)    │
└────────┬────────┘
         │
         │ 9. Persist
         ▼
┌─────────────────┐
│ ReceiptRepo     │
│ Implementation  │
└────────┬────────┘
         │
         │ 10. EF Core
         ▼
┌─────────────────┐
│   DbContext     │
└────────┬────────┘
         │
         ▼
   ┌──────────┐
   │ SQLite   │
   └──────────┘
```

---

## Module Dependency Diagram

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│                    ReceiptTracker.UI                         │
│                  (Presentation Layer)                        │
│                                                              │
└───────────┬──────────────────────────────┬───────────────────┘
            │                              │
            │ references                   │ references
            ▼                              ▼
┌───────────────────────┐      ┌──────────────────────────┐
│                       │      │                          │
│ ReceiptTracker.Services│      │  ReceiptTracker.Core    │
│  (Application Layer)  │      │    (Domain Layer)        │
│                       │      │                          │
└───────────┬───────────┘      └────────▲─────────────────┘
            │                           │
            │ references                │ references
            ▼                           │
┌───────────────────────┐               │
│                       │               │
│  ReceiptTracker.Data  ├───────────────┘
│ (Infrastructure Layer)│
│                       │
└───────────────────────┘

Dependency Rule:
- Inner layers define interfaces
- Outer layers implement interfaces
- Dependencies point inward
- Core has no dependencies
```

---

## Cross-Cutting Concerns Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Core                         │
│                                                             │
│     ┌──────────┐  ┌──────────┐  ┌──────────┐              │
│     │ Services │  │ Entities │  │   Data   │              │
│     └──────────┘  └──────────┘  └──────────┘              │
│                                                             │
└──────┬────────────┬────────────┬────────────┬──────────────┘
       │            │            │            │
       │            │            │            │
┌──────▼──────┐ ┌──▼────────┐ ┌─▼─────────┐ ┌▼──────────┐
│   Logging   │ │ Exception │ │Validation │ │   Config  │
│  (Serilog)  │ │ Handling  │ │           │ │Management │
└─────────────┘ └───────────┘ └───────────┘ └───────────┘
       │            │            │            │
       └────────────┴────────────┴────────────┘
                     │
              ┌──────▼──────┐
              │     DI      │
              │  Container  │
              └─────────────┘
```

---

## Data Flow Architecture

### Read Operations (Query)

```
View/ViewModel
      │
      ▼
  Service Layer
      │
      ▼
  Repository
      │
      ▼
  DbContext (EF Core)
      │
      ▼
  SQLite Database
      │
      ▼
  Entity/DTO
      │
      ▼
  ViewModel
      │
      ▼
  View (Data Binding)
```

### Write Operations (Command)

```
User Action
      │
      ▼
  Command/Event
      │
      ▼
  ViewModel
      │
      ▼
  Service Layer
      │
      ├─ Validation
      │
      ├─ Business Logic
      │
      ▼
  Repository
      │
      ▼
  Unit of Work
      │
      ▼
  DbContext.SaveChanges()
      │
      ▼
  SQLite Database
      │
      ▼
  Refresh UI (INotifyPropertyChanged)
```

---

## Security Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │   Input      │  │  Business    │  │    Output    │     │
│  │  Validation  │  │    Logic     │  │  Sanitization│     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────────┐
│                    Data Layer                               │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐                        │
│  │  Encrypted   │  │    API Key   │                        │
│  │   SQLite     │  │   Storage    │                        │
│  │  (Optional)  │  │  (Win Creds) │                        │
│  └──────────────┘  └──────────────┘                        │
└─────────────────────────────────────────────────────────────┘
```

---

## Deployment Architecture

```
┌─────────────────────────────────────────────────────────────┐
│              Windows Desktop Machine                        │
│                                                             │
│  ┌───────────────────────────────────────────────────────┐ │
│  │        Receipt Tracker Application                    │ │
│  │                                                       │ │
│  │  ┌──────────┐  ┌──────────┐  ┌─────────────┐        │ │
│  │  │   WPF    │  │ Business │  │   SQLite    │        │ │
│  │  │    UI    │  │  Logic   │  │   Database  │        │ │
│  │  └──────────┘  └──────────┘  └─────────────┘        │ │
│  │                                                       │ │
│  │  ┌──────────┐  ┌──────────┐                          │ │
│  │  │   Logs   │  │  Images  │                          │ │
│  │  │  Folder  │  │  Folder  │                          │ │
│  │  └──────────┘  └──────────┘                          │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                             │
│                         │                                   │
│                         │ (Optional)                        │
│                         ▼                                   │
│                ┌─────────────────┐                          │
│                │  Internet       │                          │
│                └────────┬────────┘                          │
└─────────────────────────┼───────────────────────────────────┘
                          │
                          ▼
                  ┌──────────────┐
                  │  Cloud OCR   │
                  │   Services   │
                  └──────────────┘
```

---

## Technology Stack Map

```
┌─────────────────────────────────────────────────────────────┐
│ PRESENTATION                                                │
│ ├─ WPF (.NET 8)                                             │
│ ├─ Material Design In XAML                                  │
│ ├─ MVVM Pattern                                             │
│ └─ LiveCharts2 / OxyPlot                                    │
├─────────────────────────────────────────────────────────────┤
│ APPLICATION                                                 │
│ ├─ .NET 8 C# (Latest)                                       │
│ ├─ Microsoft.Extensions.DependencyInjection                 │
│ ├─ Serilog                                                  │
│ └─ FluentValidation (Future)                                │
├─────────────────────────────────────────────────────────────┤
│ DOMAIN                                                      │
│ ├─ .NET 8 C# (Latest)                                       │
│ ├─ Pure C# (No framework dependencies)                     │
│ └─ Domain-Driven Design patterns                            │
├─────────────────────────────────────────────────────────────┤
│ INFRASTRUCTURE                                              │
│ ├─ Entity Framework Core 8.0                                │
│ ├─ SQLite                                                   │
│ ├─ Azure AI Document Intelligence (Optional)                │
│ ├─ Tesseract.NET (Offline OCR)                              │
│ └─ Windows.Media.Ocr (Offline OCR)                          │
├─────────────────────────────────────────────────────────────┤
│ TESTING                                                     │
│ ├─ xUnit                                                    │
│ ├─ Moq                                                      │
│ ├─ FluentAssertions                                         │
│ └─ EF Core InMemory (For testing)                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Key Architectural Principles

### 1. Separation of Concerns
- Each layer has a specific responsibility
- Clear boundaries between layers
- No circular dependencies

### 2. Dependency Inversion
- High-level modules don't depend on low-level modules
- Both depend on abstractions (interfaces)
- Abstractions defined in Core layer

### 3. Single Responsibility
- Each class has one reason to change
- Services focused on specific business capabilities
- Repositories focused on data access only

### 4. Open/Closed Principle
- Open for extension (e.g., pluggable OCR providers)
- Closed for modification (stable interfaces)

### 5. Interface Segregation
- Small, focused interfaces
- Clients don't depend on methods they don't use

### 6. DRY (Don't Repeat Yourself)
- Common functionality in shared services
- Reusable components and utilities

---

## Performance Considerations

### Database
- Connection pooling enabled
- Indexed columns for frequent queries
- Lazy loading for related entities
- Bulk operations for imports

### UI
- Virtualization for large lists
- Async operations for I/O
- Background threads for heavy processing
- Image thumbnail caching

### Memory
- Dispose pattern for unmanaged resources
- Weak references where appropriate
- Stream processing for large files

---

## Scalability Considerations

### Current: Single User Desktop
- Local SQLite database
- In-process services
- File-based image storage

### Future: Multi-Device
- Cloud database option
- API extraction for services
- Cloud image storage option

---

**Next Steps:**
- Define specific service interfaces
- Create entity class diagrams
- Document database schema
- Define API contracts between layers
