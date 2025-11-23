# Dependency Injection Configuration

## Receipt Tracker - DI Strategy and Setup

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## Overview

The Receipt Tracker application uses **Microsoft.Extensions.DependencyInjection** as the IoC (Inversion of Control) container. This document defines the DI strategy, service lifetimes, and registration patterns.

---

## DI Container Choice

### Microsoft.Extensions.DependencyInjection

**Why this choice:**
- Native .NET support
- Standard across .NET ecosystem
- Lightweight and performant
- Good integration with Microsoft.Extensions.Hosting
- No third-party dependencies
- Excellent for WPF applications

**Alternatives considered:**
- Autofac (more features, but heavier)
- Unity (older, less maintained)
- Simple Injector (good, but less ecosystem support)

---

## Service Lifetimes

### Lifetime Definitions

| Lifetime | Description | Use Cases |
|----------|-------------|-----------|
| **Singleton** | Single instance for app lifetime | Configuration, Logging, Caches |
| **Scoped** | Instance per scope (not used much in WPF) | Database contexts (if using scopes) |
| **Transient** | New instance every time | Services, Repositories, ViewModels |

### Lifetime Strategy for Our Application

```
Singleton:
  ├─ ILogger<T> (Serilog)
  ├─ Configuration services
  ├─ Application settings
  └─ Cache services (future)

Transient:
  ├─ ViewModels (new instance per view)
  ├─ Services (stateless)
  ├─ Repositories
  ├─ Unit of Work
  └─ OCR Providers

Scoped:
  └─ Not heavily used in desktop WPF
      (Could be used with explicit scope management)
```

---

## Service Registration Structure

### Registration by Layer

#### 1. Core Layer Registration
```csharp
// ReceiptTracker.Core doesn't register services
// It only defines interfaces
```

#### 2. Data Layer Registration
```csharp
public static class DataLayerServiceRegistration
{
    public static IServiceCollection AddDataLayer(
        this IServiceCollection services,
        string connectionString)
    {
        // DbContext
        services.AddDbContext<ReceiptTrackerDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repositories
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<IReceiptRepository, ReceiptRepository>();
        services.AddTransient<ICategoryRepository, CategoryRepository>();
        services.AddTransient<IBudgetRepository, BudgetRepository>();
        services.AddTransient<ITagRepository, TagRepository>();
        services.AddTransient<IPaymentMethodRepository, PaymentMethodRepository>();

        // Unit of Work
        services.AddTransient<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

#### 3. Service Layer Registration
```csharp
public static class ServiceLayerServiceRegistration
{
    public static IServiceCollection AddServiceLayer(
        this IServiceCollection services)
    {
        // Application Services
        services.AddTransient<IExpenseService, ExpenseService>();
        services.AddTransient<IReceiptService, ReceiptService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<IBudgetService, BudgetService>();
        services.AddTransient<IAnalyticsService, AnalyticsService>();
        services.AddTransient<IReportService, ReportService>();

        // OCR Services
        services.AddTransient<IOcrService, OcrService>();
        services.AddOcrProviders();

        // Export Services
        services.AddTransient<IExportService, ExportService>();
        services.AddTransient<ICsvExporter, CsvExporter>();
        services.AddTransient<IExcelExporter, ExcelExporter>();
        services.AddTransient<IPdfExporter, PdfExporter>();

        // Import Services
        services.AddTransient<IImportService, ImportService>();

        // File Services
        services.AddSingleton<IFileStorageService, FileStorageService>();
        services.AddTransient<IImageProcessor, ImageProcessor>();

        // AutoMapper
        services.AddAutoMapper(typeof(ServiceLayerServiceRegistration).Assembly);

        return services;
    }

    private static void AddOcrProviders(this IServiceCollection services)
    {
        // Register all OCR providers
        services.AddTransient<AzureOcrProvider>();
        services.AddTransient<TesseractOcrProvider>();
        services.AddTransient<WindowsOcrProvider>();

        // Register factory for selecting provider
        services.AddTransient<IOcrProviderFactory, OcrProviderFactory>();
    }
}
```

#### 4. UI Layer Registration
```csharp
public static class UILayerServiceRegistration
{
    public static IServiceCollection AddUILayer(
        this IServiceCollection services)
    {
        // ViewModels (Transient - new instance per view)
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ReceiptListViewModel>();
        services.AddTransient<ReceiptDetailViewModel>();
        services.AddTransient<ReceiptUploadViewModel>();
        services.AddTransient<AnalyticsViewModel>();
        services.AddTransient<BudgetViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Views (Transient)
        services.AddTransient<MainWindow>();
        services.AddTransient<DashboardView>();
        services.AddTransient<ReceiptListView>();
        services.AddTransient<ReceiptDetailView>();

        // UI Services (Singleton - shared across views)
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<IThemeService, ThemeService>();

        return services;
    }
}
```

---

## Complete Application Bootstrap

### App.xaml.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Windows;
using ReceiptTracker.Data.Extensions;
using ReceiptTracker.Services.Extensions;
using ReceiptTracker.UI.Extensions;

namespace ReceiptTracker.UI;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        // Configure Serilog first
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs/receipt-tracker-.txt",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Build the host
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json",
                    optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services, context.Configuration);
            })
            .Build();
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Get connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=receipttracker.db";

        // Register all layers
        services.AddDataLayer(connectionString);
        services.AddServiceLayer();
        services.AddUILayer();

        // Register configuration
        services.AddSingleton(configuration);

        // Register options pattern
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<OcrSettings>(configuration.GetSection("OcrSettings"));
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        Log.Information("Receipt Tracker application starting...");

        // Resolve and show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        Log.Information("Receipt Tracker application shutting down...");
        Log.CloseAndFlush();

        base.OnExit(e);
    }
}
```

---

## ViewModel Resolution Pattern

### Option 1: View-First with View Locator (Recommended)

```csharp
// MainWindow.xaml.cs
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

// Usage: Container resolves MainWindow, automatically injects MainViewModel
```

### Option 2: ViewModel-First

```csharp
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        var viewType = GetViewTypeForViewModel(typeof(TViewModel));
        var view = (Window)Activator.CreateInstance(viewType);
        view.DataContext = viewModel;
        view.Show();
    }
}
```

---

## Factory Pattern for Dynamic Resolution

### OCR Provider Factory

```csharp
public interface IOcrProviderFactory
{
    IOcrProvider GetProvider(OcrProviderType type);
}

public class OcrProviderFactory : IOcrProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OcrProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOcrProvider GetProvider(OcrProviderType type)
    {
        return type switch
        {
            OcrProviderType.Azure => _serviceProvider.GetRequiredService<AzureOcrProvider>(),
            OcrProviderType.Tesseract => _serviceProvider.GetRequiredService<TesseractOcrProvider>(),
            OcrProviderType.Windows => _serviceProvider.GetRequiredService<WindowsOcrProvider>(),
            _ => throw new ArgumentException($"Unknown OCR provider: {type}")
        };
    }
}
```

### Export Service Factory

```csharp
public interface IExportServiceFactory
{
    IExporter GetExporter(ExportFormat format);
}

public class ExportServiceFactory : IExportServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ExportServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IExporter GetExporter(ExportFormat format)
    {
        return format switch
        {
            ExportFormat.Csv => _serviceProvider.GetRequiredService<ICsvExporter>(),
            ExportFormat.Excel => _serviceProvider.GetRequiredService<IExcelExporter>(),
            ExportFormat.Pdf => _serviceProvider.GetRequiredService<IPdfExporter>(),
            _ => throw new ArgumentException($"Unknown export format: {format}")
        };
    }
}
```

---

## Configuration Options Pattern

### AppSettings.cs

```csharp
public class AppSettings
{
    public string DatabasePath { get; set; } = "receipttracker.db";
    public string ImageStoragePath { get; set; } = "images";
    public string LogPath { get; set; } = "logs";
    public int MaxImageSizeMB { get; set; } = 10;
    public bool EnableAutoBackup { get; set; } = true;
    public int BackupRetentionDays { get; set; } = 30;
}

public class OcrSettings
{
    public OcrProviderType DefaultProvider { get; set; } = OcrProviderType.Tesseract;
    public string AzureEndpoint { get; set; } = string.Empty;
    public int MinConfidenceLevel { get; set; } = 70;
    public bool FallbackToOffline { get; set; } = true;
}
```

### Usage in Services

```csharp
public class ReceiptService : IReceiptService
{
    private readonly AppSettings _appSettings;
    private readonly OcrSettings _ocrSettings;

    public ReceiptService(
        IOptions<AppSettings> appSettings,
        IOptions<OcrSettings> ocrSettings)
    {
        _appSettings = appSettings.Value;
        _ocrSettings = ocrSettings.Value;
    }
}
```

---

## Testing Support

### Service Collection for Tests

```csharp
public class TestServiceProvider
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // Add only what's needed for tests
        services.AddLogging();

        // Use in-memory database for tests
        services.AddDbContext<ReceiptTrackerDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Mock repositories or use real ones with in-memory DB
        services.AddTransient<IReceiptRepository, ReceiptRepository>();

        // Add services under test
        services.AddTransient<IReceiptService, ReceiptService>();

        return services.BuildServiceProvider();
    }
}

// Usage in tests
[Fact]
public void TestReceiptService()
{
    var serviceProvider = TestServiceProvider.BuildServiceProvider();
    var receiptService = serviceProvider.GetRequiredService<IReceiptService>();
    // Test the service
}
```

---

## Advanced Patterns

### Decorator Pattern with DI

```csharp
// Register a decorator
services.AddTransient<IOcrService, OcrService>();
services.Decorate<IOcrService, CachedOcrService>();
services.Decorate<IOcrService, LoggingOcrDecorator>();

// CachedOcrService wraps OcrService
public class CachedOcrService : IOcrService
{
    private readonly IOcrService _inner;
    private readonly IMemoryCache _cache;

    public CachedOcrService(IOcrService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<OcrResult> ProcessImageAsync(string imagePath)
    {
        var cacheKey = $"ocr_{imagePath}";
        if (_cache.TryGetValue(cacheKey, out OcrResult cached))
            return cached;

        var result = await _inner.ProcessImageAsync(imagePath);
        _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
        return result;
    }
}
```

Note: Requires Scrutor package for Decorate extension:
```xml
<PackageReference Include="Scrutor" Version="4.2.2" />
```

### Lazy Resolution

```csharp
public class ExpensiveService
{
    private readonly Lazy<IHeavyDependency> _heavyDependency;

    public ExpensiveService(Lazy<IHeavyDependency> heavyDependency)
    {
        _heavyDependency = heavyDependency;
    }

    public void UseWhenNeeded()
    {
        // Only created when accessed
        _heavyDependency.Value.DoSomething();
    }
}

// Registration
services.AddTransient<IHeavyDependency, HeavyDependency>();
```

---

## Service Provider Access

### ✅ Correct: Constructor Injection (Preferred)

```csharp
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _repository;
    private readonly IOcrService _ocrService;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        IReceiptRepository repository,
        IOcrService ocrService,
        ILogger<ReceiptService> logger)
    {
        _repository = repository;
        _ocrService = ocrService;
        _logger = logger;
    }
}
```

### ⚠️ Use Sparingly: Service Locator Pattern

```csharp
// Only for factories or when constructor injection isn't possible
public class SomeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public SomeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ISomeService Create(string type)
    {
        // Dynamic resolution based on runtime value
        return _serviceProvider.GetRequiredService<ISomeService>();
    }
}
```

### ❌ Anti-Pattern: Static Service Locator

```csharp
// DON'T DO THIS
public static class ServiceLocator
{
    public static IServiceProvider Provider { get; set; }
}

// This breaks testability and creates hidden dependencies
```

---

## Disposal and Lifetime Management

### IDisposable Services

```csharp
// Services implementing IDisposable are automatically disposed
public class FileStorageService : IFileStorageService, IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

// No manual disposal needed - DI container handles it
```

### Scoped Services in WPF

```csharp
// Create explicit scopes when needed
public async Task ProcessBatchAsync()
{
    using var scope = _serviceProvider.CreateScope();
    var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
    await scopedService.ProcessAsync();
    // Service disposed when scope ends
}
```

---

## Validation of Services

### Service Registration Validation

```csharp
protected override async void OnStartup(StartupEventArgs e)
{
    await _host.StartAsync();

    // Validate critical services are registered
    ValidateServices(_host.Services);

    // Continue startup...
}

private void ValidateServices(IServiceProvider services)
{
    var criticalServices = new[]
    {
        typeof(IReceiptService),
        typeof(IExpenseService),
        typeof(ReceiptTrackerDbContext)
    };

    foreach (var serviceType in criticalServices)
    {
        try
        {
            services.GetRequiredService(serviceType);
            Log.Information("Service {ServiceType} registered successfully", serviceType.Name);
        }
        catch (InvalidOperationException ex)
        {
            Log.Fatal("Critical service {ServiceType} not registered: {Error}",
                serviceType.Name, ex.Message);
            throw;
        }
    }
}
```

---

## Performance Considerations

### Singleton vs Transient

**Use Singleton for:**
- Stateless services
- Services that are expensive to create
- Cache services
- Configuration services

**Use Transient for:**
- Stateful services
- ViewModels (each view gets its own instance)
- Services that hold user-specific data
- Lightweight services

### Avoid Captive Dependencies

```csharp
// ❌ BAD: Singleton capturing Transient
services.AddSingleton<IMySingleton, MySingleton>();
services.AddTransient<IMyTransient, MyTransient>();

public class MySingleton : IMySingleton
{
    private readonly IMyTransient _transient; // WRONG! Becomes a singleton

    public MySingleton(IMyTransient transient)
    {
        _transient = transient;
    }
}

// ✅ GOOD: Use factory or service provider
public class MySingleton : IMySingleton
{
    private readonly IServiceProvider _serviceProvider;

    public MySingleton(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void DoWork()
    {
        using var scope = _serviceProvider.CreateScope();
        var transient = scope.ServiceProvider.GetRequiredService<IMyTransient>();
        transient.DoSomething();
    }
}
```

---

## Summary: Complete Registration Flow

```
App.xaml.cs
    └─ ConfigureServices()
        ├─ AddDataLayer(connectionString)
        │   ├─ DbContext
        │   ├─ Repositories
        │   └─ Unit of Work
        ├─ AddServiceLayer()
        │   ├─ Application Services
        │   ├─ OCR Services
        │   ├─ Export/Import Services
        │   └─ AutoMapper
        ├─ AddUILayer()
        │   ├─ ViewModels
        │   ├─ Views
        │   └─ UI Services
        └─ Configuration Options
            ├─ AppSettings
            └─ OcrSettings
```

---

**Next Steps:**
- Implement ServiceRegistration extension methods in each layer
- Create base classes with DI support
- Add validation for service registration
- Document specific service configurations
