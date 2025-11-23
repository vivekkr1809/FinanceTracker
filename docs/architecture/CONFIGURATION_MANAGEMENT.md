# Configuration Management Strategy

## Receipt Tracker - Application Configuration

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## Overview

This document defines how configuration is managed in the Receipt Tracker application, including application settings, user preferences, secrets management, and environment-specific configuration.

---

## Configuration Types

### 1. Application Settings
- **Purpose:** System-wide configuration
- **Scope:** All users
- **Storage:** appsettings.json
- **Examples:** Database connection, file paths, logging levels

### 2. User Preferences
- **Purpose:** User-specific settings
- **Scope:** Per user
- **Storage:** Database (UserSettings table)
- **Examples:** Theme, default category, currency

### 3. Secrets
- **Purpose:** Sensitive data
- **Scope:** Per installation
- **Storage:** Windows Credential Manager
- **Examples:** API keys, encryption keys

### 4. Environment Configuration
- **Purpose:** Environment-specific overrides
- **Scope:** Development, Production
- **Storage:** appsettings.{Environment}.json
- **Examples:** Debug logging, test API endpoints

---

## Application Settings (appsettings.json)

### File Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=receipttracker.db"
  },

  "AppSettings": {
    "DatabasePath": "receipttracker.db",
    "ImageStoragePath": "images",
    "LogPath": "logs",
    "MaxImageSizeMB": 10,
    "EnableAutoBackup": true,
    "BackupRetentionDays": 30,
    "BackupPath": "backups"
  },

  "OcrSettings": {
    "DefaultProvider": "Tesseract",
    "MinConfidenceLevel": 70,
    "FallbackToOffline": true,
    "MaxRetries": 3,
    "TimeoutSeconds": 30,
    "EnableCaching": true,
    "CacheDurationMinutes": 60
  },

  "AzureOcr": {
    "Endpoint": "",
    "UseManagedIdentity": false
  },

  "ImageProcessing": {
    "ThumbnailSize": 200,
    "MaxImageWidth": 4000,
    "MaxImageHeight": 4000,
    "CompressionQuality": 85,
    "SupportedFormats": ["jpg", "jpeg", "png", "heic", "pdf"]
  },

  "Export": {
    "DefaultFormat": "CSV",
    "CsvDelimiter": ",",
    "CsvEncoding": "UTF-8",
    "ExcelMaxRows": 1000000,
    "PdfPageSize": "A4"
  },

  "Analytics": {
    "DefaultPeriod": "Monthly",
    "CacheDurationMinutes": 15,
    "MaxDataPoints": 365
  },

  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning",
      "ReceiptTracker": "Debug"
    },
    "Serilog": {
      "MinimumLevel": {
        "Default": "Information",
        "Override": {
          "Microsoft": "Warning",
          "System": "Warning"
        }
      },
      "WriteTo": [
        {
          "Name": "Console"
        },
        {
          "Name": "File",
          "Args": {
            "path": "logs/receipt-tracker-.txt",
            "rollingInterval": "Day",
            "retainedFileCountLimit": 30
          }
        }
      ]
    }
  }
}
```

### Development Override (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=receipttracker-dev.db"
  },

  "AppSettings": {
    "EnableAutoBackup": false
  },

  "OcrSettings": {
    "DefaultProvider": "Tesseract",
    "EnableCaching": false
  },

  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "ReceiptTracker": "Trace"
    }
  }
}
```

---

## Configuration Classes (Options Pattern)

### AppSettings.cs

```csharp
namespace ReceiptTracker.Core.Configuration;

public class AppSettings
{
    public const string SectionName = "AppSettings";

    public string DatabasePath { get; set; } = "receipttracker.db";
    public string ImageStoragePath { get; set; } = "images";
    public string LogPath { get; set; } = "logs";
    public int MaxImageSizeMB { get; set; } = 10;
    public bool EnableAutoBackup { get; set; } = true;
    public int BackupRetentionDays { get; set; } = 30;
    public string BackupPath { get; set; } = "backups";

    public string GetFullDatabasePath()
    {
        return Path.IsPathRooted(DatabasePath)
            ? DatabasePath
            : Path.Combine(AppContext.BaseDirectory, DatabasePath);
    }

    public string GetFullImageStoragePath()
    {
        return Path.IsPathRooted(ImageStoragePath)
            ? ImageStoragePath
            : Path.Combine(AppContext.BaseDirectory, ImageStoragePath);
    }
}

public class OcrSettings
{
    public const string SectionName = "OcrSettings";

    public OcrProviderType DefaultProvider { get; set; } = OcrProviderType.Tesseract;
    public int MinConfidenceLevel { get; set; } = 70;
    public bool FallbackToOffline { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableCaching { get; set; } = true;
    public int CacheDurationMinutes { get; set; } = 60;
}

public class AzureOcrSettings
{
    public const string SectionName = "AzureOcr";

    public string Endpoint { get; set; } = string.Empty;
    public bool UseManagedIdentity { get; set; } = false;
}

public class ImageProcessingSettings
{
    public const string SectionName = "ImageProcessing";

    public int ThumbnailSize { get; set; } = 200;
    public int MaxImageWidth { get; set; } = 4000;
    public int MaxImageHeight { get; set; } = 4000;
    public int CompressionQuality { get; set; } = 85;
    public List<string> SupportedFormats { get; set; } = new() { "jpg", "jpeg", "png", "heic", "pdf" };

    public bool IsFormatSupported(string extension)
    {
        return SupportedFormats.Contains(extension.TrimStart('.').ToLowerInvariant());
    }
}

public class ExportSettings
{
    public const string SectionName = "Export";

    public string DefaultFormat { get; set; } = "CSV";
    public string CsvDelimiter { get; set; } = ",";
    public string CsvEncoding { get; set; } = "UTF-8";
    public int ExcelMaxRows { get; set; } = 1_000_000;
    public string PdfPageSize { get; set; } = "A4";
}

public class AnalyticsSettings
{
    public const string SectionName = "Analytics";

    public string DefaultPeriod { get; set; } = "Monthly";
    public int CacheDurationMinutes { get; set; } = 15;
    public int MaxDataPoints { get; set; } = 365;
}
```

### Registration in DI

```csharp
// In App.xaml.cs or Startup
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // Register configuration sections
    services.Configure<AppSettings>(
        configuration.GetSection(AppSettings.SectionName));

    services.Configure<OcrSettings>(
        configuration.GetSection(OcrSettings.SectionName));

    services.Configure<AzureOcrSettings>(
        configuration.GetSection(AzureOcrSettings.SectionName));

    services.Configure<ImageProcessingSettings>(
        configuration.GetSection(ImageProcessingSettings.SectionName));

    services.Configure<ExportSettings>(
        configuration.GetSection(ExportSettings.SectionName));

    services.Configure<AnalyticsSettings>(
        configuration.GetSection(AnalyticsSettings.SectionName));

    // Enable options validation
    services.AddOptions<AppSettings>()
        .Bind(configuration.GetSection(AppSettings.SectionName))
        .ValidateDataAnnotations()
        .ValidateOnStart();
}
```

### Usage in Services

```csharp
public class ReceiptService : IReceiptService
{
    private readonly AppSettings _appSettings;
    private readonly OcrSettings _ocrSettings;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        IOptions<AppSettings> appSettings,
        IOptions<OcrSettings> ocrSettings,
        ILogger<ReceiptService> logger)
    {
        _appSettings = appSettings.Value;
        _ocrSettings = ocrSettings.Value;
        _logger = logger;
    }

    public async Task ProcessReceiptAsync(string imagePath)
    {
        var maxSize = _appSettings.MaxImageSizeMB * 1024 * 1024;
        var fileInfo = new FileInfo(imagePath);

        if (fileInfo.Length > maxSize)
        {
            throw new ValidationException(
                nameof(imagePath),
                $"Image size exceeds maximum allowed size of {_appSettings.MaxImageSizeMB}MB");
        }

        // Process with configured OCR provider
        var provider = GetOcrProvider(_ocrSettings.DefaultProvider);
        await provider.ProcessAsync(imagePath);
    }
}
```

---

## User Preferences (Database)

### UserSettings Entity

```csharp
namespace ReceiptTracker.Core.Domain.Entities;

public class UserSettings : Entity
{
    public string UserId { get; set; } = "default"; // For future multi-user support
    public string Theme { get; set; } = "Light";
    public string Currency { get; set; } = "USD";
    public int? DefaultCategoryId { get; set; }
    public Category? DefaultCategory { get; set; }
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public DayOfWeek WeekStartDay { get; set; } = DayOfWeek.Sunday;
    public bool EnableNotifications { get; set; } = true;
    public bool AutoCategorize { get; set; } = true;
    public bool ShowBudgetAlerts { get; set; } = true;
    public int BudgetAlertThreshold { get; set; } = 90; // Percentage
    public string Language { get; set; } = "en-US";

    // Display preferences
    public int ItemsPerPage { get; set; } = 50;
    public string DefaultViewMode { get; set; } = "List"; // List, Grid, Tiles
    public bool ShowThumbnails { get; set; } = true;

    // Export preferences
    public string PreferredExportFormat { get; set; } = "CSV";
    public bool IncludeImagesInExport { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### User Settings Service

```csharp
public interface IUserSettingsService
{
    Task<UserSettings> GetUserSettingsAsync();
    Task UpdateUserSettingsAsync(UserSettings settings);
    Task<T> GetSettingAsync<T>(string key, T defaultValue);
    Task SetSettingAsync<T>(string key, T value);
}

public class UserSettingsService : IUserSettingsService
{
    private readonly IUserSettingsRepository _repository;
    private readonly ILogger<UserSettingsService> _logger;
    private UserSettings? _cachedSettings;

    public UserSettingsService(
        IUserSettingsRepository repository,
        ILogger<UserSettingsService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserSettings> GetUserSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        _cachedSettings = await _repository.GetByUserIdAsync("default")
            ?? await CreateDefaultSettingsAsync();

        return _cachedSettings;
    }

    public async Task UpdateUserSettingsAsync(UserSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;
        await _repository.UpdateAsync(settings);
        await _repository.UnitOfWork.SaveChangesAsync();

        _cachedSettings = settings;
        _logger.LogInformation("User settings updated");
    }

    private async Task<UserSettings> CreateDefaultSettingsAsync()
    {
        var settings = new UserSettings
        {
            UserId = "default",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(settings);
        await _repository.UnitOfWork.SaveChangesAsync();

        return settings;
    }
}
```

### Usage in ViewModels

```csharp
public class SettingsViewModel : ViewModelBase
{
    private readonly IUserSettingsService _userSettingsService;
    private UserSettings _settings;

    public string SelectedTheme
    {
        get => _settings.Theme;
        set
        {
            if (_settings.Theme != value)
            {
                _settings.Theme = value;
                OnPropertyChanged();
            }
        }
    }

    public async Task LoadSettingsAsync()
    {
        _settings = await _userSettingsService.GetUserSettingsAsync();
        OnPropertyChanged(nameof(SelectedTheme));
        OnPropertyChanged(nameof(Currency));
        // ... other properties
    }

    public async Task SaveSettingsAsync()
    {
        await _userSettingsService.UpdateUserSettingsAsync(_settings);
    }
}
```

---

## Secrets Management

### Windows Credential Manager

```csharp
public interface ISecretStore
{
    Task<string?> GetSecretAsync(string key);
    Task SetSecretAsync(string key, string value);
    Task DeleteSecretAsync(string key);
}

public class WindowsCredentialStore : ISecretStore
{
    private const string TargetPrefix = "ReceiptTracker_";

    public Task<string?> GetSecretAsync(string key)
    {
        return Task.Run(() =>
        {
            try
            {
                using var credential = new Credential
                {
                    Target = GetTargetName(key)
                };
                credential.Load();
                return credential.Password;
            }
            catch
            {
                return null;
            }
        });
    }

    public Task SetSecretAsync(string key, string value)
    {
        return Task.Run(() =>
        {
            using var credential = new Credential
            {
                Target = GetTargetName(key),
                Password = value,
                PersistanceType = PersistanceType.LocalComputer
            };
            credential.Save();
        });
    }

    public Task DeleteSecretAsync(string key)
    {
        return Task.Run(() =>
        {
            using var credential = new Credential
            {
                Target = GetTargetName(key)
            };
            credential.Delete();
        });
    }

    private static string GetTargetName(string key)
    {
        return $"{TargetPrefix}{key}";
    }
}
```

### Usage for API Keys

```csharp
public class AzureOcrProvider : IOcrProvider
{
    private readonly ISecretStore _secretStore;
    private readonly AzureOcrSettings _settings;
    private string? _apiKey;

    public async Task<OcrResult> ProcessImageAsync(string imagePath)
    {
        // Lazy load API key from credential store
        _apiKey ??= await _secretStore.GetSecretAsync("AzureOcrApiKey");

        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new ConfigurationException(
                "Azure OCR API key not configured. Please set it in Settings.");
        }

        // Use API key for OCR processing
        var client = new DocumentAnalysisClient(
            new Uri(_settings.Endpoint),
            new AzureKeyCredential(_apiKey));

        // ... process image
    }
}
```

---

## Configuration Validation

### Data Annotations

```csharp
using System.ComponentModel.DataAnnotations;

public class AppSettings
{
    [Required]
    [MinLength(1)]
    public string DatabasePath { get; set; } = "receipttracker.db";

    [Required]
    [MinLength(1)]
    public string ImageStoragePath { get; set; } = "images";

    [Range(1, 100)]
    public int MaxImageSizeMB { get; set; } = 10;

    [Range(1, 365)]
    public int BackupRetentionDays { get; set; } = 30;
}
```

### Custom Validation

```csharp
public class OcrSettingsValidator : IValidateOptions<OcrSettings>
{
    public ValidateOptionsResult Validate(string name, OcrSettings options)
    {
        var errors = new List<string>();

        if (options.MinConfidenceLevel < 0 || options.MinConfidenceLevel > 100)
        {
            errors.Add("MinConfidenceLevel must be between 0 and 100");
        }

        if (options.TimeoutSeconds < 1)
        {
            errors.Add("TimeoutSeconds must be greater than 0");
        }

        if (errors.Any())
        {
            return ValidateOptionsResult.Fail(errors);
        }

        return ValidateOptionsResult.Success;
    }
}

// Register validator
services.AddSingleton<IValidateOptions<OcrSettings>, OcrSettingsValidator>();
```

---

## Configuration Reload

### Hot Reload Support

```csharp
public class AppSettings
{
    // Use IOptionsSnapshot for hot reload
}

public class SomeService
{
    private readonly IOptionsSnapshot<AppSettings> _appSettings;

    public SomeService(IOptionsSnapshot<AppSettings> appSettings)
    {
        _appSettings = appSettings;
    }

    public void DoSomething()
    {
        // Always gets current value (reloaded if file changed)
        var currentValue = _appSettings.Value.MaxImageSizeMB;
    }
}
```

### Configuration Change Notifications

```csharp
public class ConfigurationMonitor
{
    private readonly IOptionsMonitor<AppSettings> _appSettings;
    private readonly ILogger<ConfigurationMonitor> _logger;

    public ConfigurationMonitor(
        IOptionsMonitor<AppSettings> appSettings,
        ILogger<ConfigurationMonitor> logger)
    {
        _appSettings = appSettings;
        _logger = logger;

        // Subscribe to changes
        _appSettings.OnChange(OnConfigurationChanged);
    }

    private void OnConfigurationChanged(AppSettings newSettings)
    {
        _logger.LogInformation("Application settings changed. MaxImageSizeMB: {Size}",
            newSettings.MaxImageSizeMB);

        // React to changes
        // e.g., reinitialize services, update cache, etc.
    }
}
```

---

## Environment Detection

```csharp
public static class EnvironmentHelper
{
    public static bool IsDevelopment()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }

    public static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? (IsDevelopment() ? "Development" : "Production");
    }
}
```

---

## Configuration Migration

### Handling Version Changes

```csharp
public class ConfigurationMigration
{
    private const int CurrentVersion = 2;

    public async Task MigrateIfNeededAsync()
    {
        var version = await GetConfigVersionAsync();

        if (version < CurrentVersion)
        {
            await MigrateFromVersion(version);
            await SetConfigVersionAsync(CurrentVersion);
        }
    }

    private async Task MigrateFromVersion(int fromVersion)
    {
        // Version 1 -> 2: Move settings from registry to database
        if (fromVersion == 1)
        {
            // Migration logic
        }
    }
}
```

---

## Configuration Best Practices

### ✅ Do

1. Use strongly-typed configuration classes
2. Validate configuration on startup
3. Use Options pattern (IOptions, IOptionsSnapshot, IOptionsMonitor)
4. Store secrets in secure storage (not in config files)
5. Use environment-specific config files
6. Document all configuration options
7. Provide sensible defaults
8. Use constants for section names

### ❌ Don't

1. Hardcode values in code
2. Store secrets in appsettings.json
3. Access configuration directly everywhere (use DI)
4. Use magic strings for keys
5. Forget to validate configuration
6. Leave configuration files in source control with real secrets
7. Use configuration for runtime data

---

## Configuration File Locations

```
Application Root/
├── appsettings.json                    (Checked into source control)
├── appsettings.Development.json        (Checked into source control)
├── appsettings.Production.json         (Checked into source control)
├── appsettings.local.json             (NOT in source control, user-specific)
└── %APPDATA%/ReceiptTracker/
    └── user-settings.json             (User-specific preferences)
```

### .gitignore

```
appsettings.local.json
*.user.json
secrets.json
```

---

## Summary

### Configuration Strategy

| Type | Storage | Scope | Example |
|------|---------|-------|---------|
| **App Settings** | appsettings.json | Application-wide | Database path, logging |
| **User Preferences** | Database | Per user | Theme, currency |
| **Secrets** | Credential Manager | Per installation | API keys |
| **Environment** | Environment files | Per environment | Debug flags |

### Access Pattern

```
Configuration Source
        ↓
    IOptions<T>
        ↓
  Injected into Services
        ↓
    Used in Business Logic
```

---

**Next Steps:**
- Create configuration classes
- Implement user settings service
- Implement secret store
- Add configuration validation
- Create settings UI
