# Receipt Tracker - Personal Finance Windows Desktop Application

A modern, feature-rich Windows desktop application for tracking expenses through receipt scanning and OCR processing. Built with WPF and .NET 8.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-Latest-239120?logo=c-sharp)
![WPF](https://img.shields.io/badge/WPF-Windows-0078D4?logo=windows)
![License](https://img.shields.io/badge/License-MIT-green)

## Overview

Receipt Tracker is a comprehensive personal finance application that helps you:
- 📸 **Scan and digitize receipts** using OCR technology
- 💰 **Track expenses** automatically with intelligent categorization
- 📊 **Visualize spending** with interactive charts and reports
- 🎯 **Manage budgets** with real-time alerts and insights
- 📱 **Export data** to various formats (CSV, Excel, PDF)
- 🔒 **Keep data private** with local SQLite database

## Current Status

**Phase 1.1: Project Foundation - COMPLETED** ✅

This is the initial setup phase. The application foundation is established with:
- Project structure and architecture
- Build pipeline and CI/CD
- Code quality tools and standards
- Logging infrastructure

## Features (Planned)

### Core Features
- 🖼️ Receipt image processing (JPEG, PNG, HEIC, PDF)
- 🔍 OCR-based text extraction and data parsing
- 🏷️ Smart expense categorization
- 💳 Payment method tracking
- 🔖 Tag-based organization
- 🔎 Advanced search and filtering
- 📈 Analytics and spending insights
- 💵 Budget management with alerts

### Analytics & Reporting
- Daily, weekly, monthly, quarterly, and yearly summaries
- Category-wise spending breakdowns
- Trend analysis and spending patterns
- Budget vs actual comparisons
- Custom date range reports
- Export to PDF, CSV, and Excel

### User Experience
- Modern Material Design interface
- Drag-and-drop receipt upload
- Clipboard paste support
- Keyboard shortcuts
- Light/Dark theme support
- Responsive and intuitive UI

## Technology Stack

| Component | Technology |
|-----------|-----------|
| **UI Framework** | WPF (Windows Presentation Foundation) |
| **Runtime** | .NET 8.0 |
| **Language** | C# (Latest) |
| **Database** | SQLite with Entity Framework Core 8.0 |
| **UI Styling** | Material Design In XAML |
| **Charts** | LiveCharts2 / OxyPlot |
| **Logging** | Serilog |
| **Testing** | xUnit, Moq, FluentAssertions |
| **CI/CD** | GitHub Actions |

## Project Structure

```
FinanceTracker/
├── src/
│   ├── ReceiptTracker.Core/          # Domain models, entities, interfaces
│   ├── ReceiptTracker.Data/          # EF Core, repositories, data access
│   ├── ReceiptTracker.Services/      # Business logic, OCR, analytics
│   └── ReceiptTracker.UI/            # WPF application, views, view models
├── tests/
│   ├── ReceiptTracker.Core.Tests/
│   ├── ReceiptTracker.Data.Tests/
│   └── ReceiptTracker.Services.Tests/
├── docs/
│   ├── architecture/                 # Architecture decisions and diagrams
│   ├── DEVELOPMENT_REQUIREMENTS.md   # Setup instructions
│   └── api/                          # API documentation
└── ReceiptTracker.sln                # Visual Studio solution
```

## Getting Started

### Prerequisites

- **Operating System**: Windows 10 (1809+) or Windows 11
- **.NET 8.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **IDE**: Visual Studio 2022 (recommended) or Visual Studio Code
- **Git**: For version control

For detailed requirements, see [Development Requirements](docs/DEVELOPMENT_REQUIREMENTS.md).

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/FinanceTracker.git
   cd FinanceTracker
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/ReceiptTracker.UI/ReceiptTracker.UI.csproj
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Development Workflow

### Code Style
The project uses EditorConfig for consistent code formatting. Your IDE should automatically apply these settings. Key standards:
- 4 spaces for indentation
- Nullable reference types enabled
- Private fields prefixed with underscore: `_fieldName`
- Interfaces prefixed with I: `IRepository`

### Commit Guidelines
- Write clear, descriptive commit messages
- Use conventional commit format when possible
- Keep commits focused and atomic

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch for features
- `claude/*` - Feature branches for development

## Documentation

- [Architecture Decision: UI Framework](docs/architecture/UI_FRAMEWORK_DECISION.md)
- [Logging Strategy](docs/architecture/LOGGING_STRATEGY.md)
- [Development Requirements](docs/DEVELOPMENT_REQUIREMENTS.md)

## Development Phases

The application is being built in phases:

- ✅ **Phase 1.1**: Project Setup - *Completed*
- ⏳ **Phase 1.2**: Architecture Design - *Next*
- 📋 **Phase 2**: Database Layer
- 📋 **Phase 3**: Receipt Image Processing
- 📋 **Phase 4**: Core Business Logic
- 📋 **Phase 5**: Analytics & Insights Engine
- 📋 **Phase 6**: User Interface
- 📋 **Phase 7**: Data Import/Export
- 📋 **Phase 8**: Security & Privacy
- 📋 **Phase 9**: Performance & Reliability
- 📋 **Phase 10**: Testing
- 📋 **Phase 11**: Deployment & Distribution
- 📋 **Phase 12**: Documentation
- 📋 **Phase 13**: Post-Launch & Maintenance

See the full task list in the project planning documents.

## Contributing

Contributions are welcome! Please follow these guidelines:
1. Fork the repository
2. Create a feature branch
3. Follow the code style guidelines
4. Write tests for new features
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

### Immediate Next Steps (Phase 1.2)
- [ ] Document high-level architecture diagram
- [ ] Define layered architecture boundaries
- [ ] Design dependency injection configuration
- [ ] Define error handling strategy
- [ ] Design configuration management approach
- [ ] Document data flow for receipt processing pipeline

### Future Enhancements (Post-MVP)
- ☁️ Cloud sync across devices
- 📱 Mobile companion app
- 📧 Receipt email forwarding integration
- 🏦 Bank account integration
- 💱 Multi-currency support with conversion
- 👥 Shared expenses / expense splitting
- 📑 Tax report generation
- 🔗 Integration with accounting software (QuickBooks, Xero)
- 🎤 Voice input for quick expense entry
- 🪟 Windows desktop widget

## Support

For issues, questions, or suggestions:
- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/yourusername/FinanceTracker/issues)
- 💡 **Feature Requests**: [GitHub Discussions](https://github.com/yourusername/FinanceTracker/discussions)
- 📧 **Email**: support@example.com

## Acknowledgments

- [Material Design In XAML](http://materialdesigninxaml.net/) for beautiful UI components
- [Serilog](https://serilog.net/) for structured logging
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access
- The .NET community for excellent tools and libraries

---

**Built with ❤️ using .NET 8 and WPF**

*Last Updated: 2025-11-23*
