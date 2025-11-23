# Development Environment Requirements

## Overview
This document outlines the software and tools required to develop, build, and run the Receipt Tracker application.

## Operating System
- **Windows 10** (version 1809 or later) or **Windows 11**
- Required for WPF application development and testing

## Required Software

### .NET SDK
- **.NET 8.0 SDK** or later
- Download from: https://dotnet.microsoft.com/download/dotnet/8.0
- Verify installation: `dotnet --version`

### IDE / Code Editor
Choose one of the following:

#### Option 1: Visual Studio 2022 (Recommended)
- **Version:** Visual Studio 2022 (17.8 or later)
- **Edition:** Community (free), Professional, or Enterprise
- **Required Workloads:**
  - .NET desktop development
  - .NET Multi-platform App UI development (optional, for future MAUI support)
- **Recommended Extensions:**
  - ReSharper or CodeMaid (code cleanup)
  - GitFlow (version control)
  - Markdown Editor
- Download from: https://visualstudio.microsoft.com/downloads/

#### Option 2: Visual Studio Code
- **Version:** Latest stable version
- **Required Extensions:**
  - C# Dev Kit
  - .NET Extension Pack
  - Material Design Icons
  - GitLens
- Download from: https://code.visualstudio.com/

#### Option 3: JetBrains Rider
- **Version:** 2023.3 or later
- Download from: https://www.jetbrains.com/rider/

### Version Control
- **Git** (version 2.30 or later)
- Download from: https://git-scm.com/downloads
- Verify installation: `git --version`

### Optional Tools

#### Database Management
- **DB Browser for SQLite** - For viewing and managing SQLite databases
  - Download from: https://sqlitebrowser.org/

#### API Testing (for future cloud OCR integration)
- **Postman** or **Insomnia** - For testing API endpoints
  - Postman: https://www.postman.com/downloads/
  - Insomnia: https://insomnia.rest/download

#### Image Processing Testing
- **IrfanView** or similar - For testing various image formats
  - Download from: https://www.irfanview.com/

## Build Environment

### NuGet Package Restore
The project uses NuGet packages that will be automatically restored during build. Ensure NuGet package restore is enabled in your IDE.

### Key Dependencies
The following packages will be automatically restored:
- **Entity Framework Core 8.0** - Database ORM
- **SQLite** - Embedded database
- **Serilog** - Logging framework
- **MaterialDesignThemes** - UI controls and styling
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework for tests
- **FluentAssertions** - Assertion library for tests

## Runtime Requirements (for end users)

### Minimum System Requirements
- **OS:** Windows 10 (1809+) or Windows 11
- **.NET Runtime:** .NET 8.0 Desktop Runtime
- **RAM:** 4 GB minimum, 8 GB recommended
- **Disk Space:** 500 MB for application and database
- **Display:** 1280x720 minimum resolution

### Recommended System Requirements
- **OS:** Windows 11
- **RAM:** 8 GB or more
- **Disk Space:** 2 GB (for application, database, and receipt images)
- **Display:** 1920x1080 or higher

## Development Setup Steps

1. **Install .NET 8.0 SDK**
   ```bash
   # Verify installation
   dotnet --version
   ```

2. **Install Visual Studio 2022** (or your preferred IDE)

3. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd FinanceTracker
   ```

4. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

5. **Build the solution**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run --project src/ReceiptTracker.UI/ReceiptTracker.UI.csproj
   ```

7. **Run tests**
   ```bash
   dotnet test
   ```

## Code Style and Formatting

### EditorConfig
The project includes an `.editorconfig` file that enforces consistent code style across the team. Most modern IDEs will automatically apply these settings.

### Code Analysis
The project is configured with:
- **TreatWarningsAsErrors:** Enabled in Release builds
- **Nullable reference types:** Enabled across all projects
- **Latest C# language version:** Enabled

## Troubleshooting

### Common Issues

#### Issue: Build fails with "SDK not found"
**Solution:** Ensure .NET 8.0 SDK is installed. Run `dotnet --list-sdks` to verify.

#### Issue: NuGet restore fails
**Solution:** Clear NuGet cache with `dotnet nuget locals all --clear` and retry.

#### Issue: WPF designer not loading
**Solution:** Ensure .NET desktop development workload is installed in Visual Studio.

#### Issue: Missing MaterialDesignThemes resources
**Solution:** Clean and rebuild the solution. The package should restore automatically.

## CI/CD Environment

### GitHub Actions
The project uses GitHub Actions for continuous integration. The workflow runs on:
- **OS:** windows-latest
- **.NET SDK:** 8.0.x
- **Triggers:** Push to main/develop/claude/** branches, Pull requests to main/develop

### Build Pipeline
1. Checkout code
2. Setup .NET 8.0
3. Restore dependencies
4. Build solution (Release configuration)
5. Run tests with code coverage
6. Upload coverage reports
7. Create release package (on main branch)

## Additional Resources

- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8)
- [WPF Documentation](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [Material Design In XAML Toolkit](http://materialdesigninxaml.net/)
- [Serilog Documentation](https://serilog.net/)

## Support

For development environment issues, please:
1. Check the troubleshooting section above
2. Review the project's issue tracker on GitHub
3. Contact the development team

---

**Last Updated:** 2025-11-23
**Document Version:** 1.0
