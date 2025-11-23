# Changelog

All notable changes to the Receipt Tracker project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Planned
- Architecture design documentation (Phase 1.2)
- Database schema and EF Core setup (Phase 2)
- Receipt image processing and OCR integration (Phase 3)
- Core business logic implementation (Phase 4)

## [0.1.0] - 2025-11-23

### Added - Phase 1.1: Project Foundation
- Initial project structure with layered architecture
  - ReceiptTracker.Core - Domain layer
  - ReceiptTracker.Data - Data access layer
  - ReceiptTracker.Services - Business logic layer
  - ReceiptTracker.UI - WPF presentation layer
- Test projects for each layer
  - ReceiptTracker.Core.Tests
  - ReceiptTracker.Data.Tests
  - ReceiptTracker.Services.Tests
- GitHub Actions CI/CD pipeline
  - Automated builds on push and pull requests
  - Unit test execution
  - Code coverage reporting
  - Release package creation
- Code quality tools
  - EditorConfig for consistent code style
  - Microsoft.CodeAnalysis.NetAnalyzers
  - Directory.Build.props for global settings
- Logging framework (Serilog)
  - Console and file logging
  - Structured logging configuration
  - LoggingConfiguration helper class
- Documentation
  - Comprehensive README.md
  - Development requirements document
  - UI framework decision rationale
  - Logging strategy documentation
  - Architecture overview
- Basic WPF application shell
  - App.xaml with Material Design theme integration
  - MainWindow with basic layout
  - Dependency injection setup
- Project files and configuration
  - .gitignore for .NET and Visual Studio
  - .gitattributes for line ending handling
  - NuGet package references
  - Material Design In XAML theme setup

### Technical Details
- Target Framework: .NET 8.0
- UI Framework: WPF (Windows Presentation Foundation)
- Database: SQLite with Entity Framework Core 8.0 (configured, not yet implemented)
- Logging: Serilog 3.1.1
- UI Theme: Material Design In XAML 4.9.0
- Testing: xUnit 2.6.2, Moq 4.20.70, FluentAssertions 6.12.0

### Development Infrastructure
- Nullable reference types enabled
- Implicit usings enabled
- TreatWarningsAsErrors enabled for Release builds
- Code analysis with .NET analyzers

## Development Phases

The project follows a phased development approach:

- **Phase 1.1** ✅ - Project Setup (COMPLETED)
- **Phase 1.2** 🚧 - Architecture Design (NEXT)
- **Phase 2** 📋 - Database Layer
- **Phase 3** 📋 - Receipt Image Processing
- **Phase 4** 📋 - Core Business Logic
- **Phase 5** 📋 - Analytics & Insights Engine
- **Phase 6** 📋 - User Interface
- **Phase 7** 📋 - Data Import/Export
- **Phase 8** 📋 - Security & Privacy
- **Phase 9** 📋 - Performance & Reliability
- **Phase 10** 📋 - Testing
- **Phase 11** 📋 - Deployment & Distribution
- **Phase 12** 📋 - Documentation
- **Phase 13** 📋 - Post-Launch & Maintenance

---

[Unreleased]: https://github.com/yourusername/FinanceTracker/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/yourusername/FinanceTracker/releases/tag/v0.1.0
