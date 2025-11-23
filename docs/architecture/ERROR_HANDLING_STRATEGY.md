# Error Handling Strategy

## Receipt Tracker - Exception Management and Error Handling

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## Overview

This document defines the error handling strategy for the Receipt Tracker application, including custom exception types, exception flow, logging, and user communication patterns.

---

## Error Handling Principles

### 1. Fail Fast
- Validate early and throw exceptions immediately
- Don't let invalid state propagate
- Use guard clauses

### 2. Specific Exceptions
- Create specific exception types for different error scenarios
- Avoid generic Exception
- Make exceptions meaningful

### 3. Consistent Logging
- Log all exceptions with context
- Include correlation IDs for tracking
- Different log levels for different severities

### 4. User-Friendly Messages
- Never show technical stack traces to users
- Translate exceptions to user-friendly messages
- Provide actionable guidance

### 5. Never Swallow Exceptions
- Always log before catching
- Re-throw if you can't handle
- Use finally for cleanup

---

## Exception Hierarchy

```
System.Exception
    │
    ├─ ReceiptTrackerException (abstract base)
    │   │
    │   ├─ DomainException (domain/business logic)
    │   │   ├─ EntityNotFoundException
    │   │   ├─ DuplicateEntityException
    │   │   ├─ ValidationException
    │   │   ├─ BusinessRuleViolationException
    │   │   └─ InvalidOperationException
    │   │
    │   ├─ ApplicationException (service layer)
    │   │   ├─ ServiceException
    │   │   ├─ OcrProcessingException
    │   │   ├─ ImageProcessingException
    │   │   ├─ ExportException
    │   │   └─ ImportException
    │   │
    │   ├─ DataException (data/infrastructure)
    │   │   ├─ RepositoryException
    │   │   ├─ DatabaseException
    │   │   ├─ ConnectionException
    │   │   └─ MigrationException
    │   │
    │   └─ InfrastructureException
    │       ├─ FileStorageException
    │       ├─ ExternalServiceException
    │       └─ ConfigurationException
```

---

## Exception Definitions

### Base Exception

```csharp
namespace ReceiptTracker.Core.Exceptions;

/// <summary>
/// Base exception for all Receipt Tracker exceptions
/// </summary>
public abstract class ReceiptTrackerException : Exception
{
    public string ErrorCode { get; }
    public Dictionary<string, object> Data { get; }

    protected ReceiptTrackerException(
        string message,
        string errorCode,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        Data = new Dictionary<string, object>();
    }

    public void AddData(string key, object value)
    {
        Data[key] = value;
    }
}
```

### Domain Exceptions

```csharp
namespace ReceiptTracker.Core.Exceptions;

/// <summary>
/// Base exception for domain layer errors
/// </summary>
public class DomainException : ReceiptTrackerException
{
    public DomainException(string message, string errorCode, Exception? innerException = null)
        : base(message, errorCode, innerException)
    {
    }
}

/// <summary>
/// Thrown when an entity is not found
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.", "ENTITY_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
        AddData(nameof(EntityType), entityType);
        AddData(nameof(EntityId), entityId);
    }
}

/// <summary>
/// Thrown when a duplicate entity is detected
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string DuplicateField { get; }
    public object DuplicateValue { get; }

    public DuplicateEntityException(string entityType, string duplicateField, object duplicateValue)
        : base($"{entityType} with {duplicateField} '{duplicateValue}' already exists.", "DUPLICATE_ENTITY")
    {
        EntityType = entityType;
        DuplicateField = duplicateField;
        DuplicateValue = duplicateValue;
        AddData(nameof(EntityType), entityType);
        AddData(nameof(DuplicateField), duplicateField);
        AddData(nameof(DuplicateValue), duplicateValue);
    }
}

/// <summary>
/// Thrown when validation fails
/// </summary>
public class ValidationException : DomainException
{
    public List<ValidationError> Errors { get; }

    public ValidationException(string message, List<ValidationError> errors)
        : base(message, "VALIDATION_FAILED")
    {
        Errors = errors;
        AddData(nameof(Errors), errors);
    }

    public ValidationException(string propertyName, string errorMessage)
        : this("Validation failed", new List<ValidationError>
        {
            new ValidationError(propertyName, errorMessage)
        })
    {
    }
}

public record ValidationError(string PropertyName, string ErrorMessage);

/// <summary>
/// Thrown when a business rule is violated
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleViolationException(string ruleName, string message)
        : base(message, "BUSINESS_RULE_VIOLATION")
    {
        RuleName = ruleName;
        AddData(nameof(RuleName), ruleName);
    }
}
```

### Application/Service Exceptions

```csharp
namespace ReceiptTracker.Services.Exceptions;

/// <summary>
/// Base exception for application/service layer
/// </summary>
public class ApplicationException : ReceiptTrackerException
{
    public ApplicationException(string message, string errorCode, Exception? innerException = null)
        : base(message, errorCode, innerException)
    {
    }
}

/// <summary>
/// Thrown when OCR processing fails
/// </summary>
public class OcrProcessingException : ApplicationException
{
    public string ImagePath { get; }
    public string ProviderName { get; }

    public OcrProcessingException(string imagePath, string providerName, Exception? innerException = null)
        : base($"OCR processing failed for image '{imagePath}' using provider '{providerName}'.",
            "OCR_PROCESSING_FAILED", innerException)
    {
        ImagePath = imagePath;
        ProviderName = providerName;
        AddData(nameof(ImagePath), imagePath);
        AddData(nameof(ProviderName), providerName);
    }
}

/// <summary>
/// Thrown when image processing fails
/// </summary>
public class ImageProcessingException : ApplicationException
{
    public string ImagePath { get; }

    public ImageProcessingException(string imagePath, string message, Exception? innerException = null)
        : base($"Image processing failed: {message}", "IMAGE_PROCESSING_FAILED", innerException)
    {
        ImagePath = imagePath;
        AddData(nameof(ImagePath), imagePath);
    }
}

/// <summary>
/// Thrown when export operation fails
/// </summary>
public class ExportException : ApplicationException
{
    public string ExportFormat { get; }

    public ExportException(string exportFormat, string message, Exception? innerException = null)
        : base($"Export to {exportFormat} failed: {message}", "EXPORT_FAILED", innerException)
    {
        ExportFormat = exportFormat;
        AddData(nameof(ExportFormat), exportFormat);
    }
}

/// <summary>
/// Thrown when import operation fails
/// </summary>
public class ImportException : ApplicationException
{
    public string FilePath { get; }
    public int LineNumber { get; }

    public ImportException(string filePath, int lineNumber, string message, Exception? innerException = null)
        : base($"Import failed at line {lineNumber} in file '{filePath}': {message}",
            "IMPORT_FAILED", innerException)
    {
        FilePath = filePath;
        LineNumber = lineNumber;
        AddData(nameof(FilePath), filePath);
        AddData(nameof(LineNumber), lineNumber);
    }
}
```

### Data/Infrastructure Exceptions

```csharp
namespace ReceiptTracker.Data.Exceptions;

/// <summary>
/// Base exception for data layer
/// </summary>
public class DataException : ReceiptTrackerException
{
    public DataException(string message, string errorCode, Exception? innerException = null)
        : base(message, errorCode, innerException)
    {
    }
}

/// <summary>
/// Thrown when repository operation fails
/// </summary>
public class RepositoryException : DataException
{
    public string RepositoryName { get; }
    public string Operation { get; }

    public RepositoryException(string repositoryName, string operation, Exception? innerException = null)
        : base($"Repository operation '{operation}' failed in {repositoryName}.",
            "REPOSITORY_OPERATION_FAILED", innerException)
    {
        RepositoryName = repositoryName;
        Operation = operation;
        AddData(nameof(RepositoryName), repositoryName);
        AddData(nameof(Operation), operation);
    }
}

/// <summary>
/// Thrown when database operation fails
/// </summary>
public class DatabaseException : DataException
{
    public DatabaseException(string message, Exception? innerException = null)
        : base(message, "DATABASE_ERROR", innerException)
    {
    }
}

/// <summary>
/// Thrown when file storage operation fails
/// </summary>
public class FileStorageException : DataException
{
    public string FilePath { get; }
    public string Operation { get; }

    public FileStorageException(string filePath, string operation, Exception? innerException = null)
        : base($"File storage operation '{operation}' failed for '{filePath}'.",
            "FILE_STORAGE_FAILED", innerException)
    {
        FilePath = filePath;
        Operation = operation;
        AddData(nameof(FilePath), filePath);
        AddData(nameof(Operation), operation);
    }
}

/// <summary>
/// Thrown when external service call fails
/// </summary>
public class ExternalServiceException : DataException
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public ExternalServiceException(string serviceName, int? statusCode = null, Exception? innerException = null)
        : base($"External service '{serviceName}' failed" +
            (statusCode.HasValue ? $" with status code {statusCode}" : "") + ".",
            "EXTERNAL_SERVICE_FAILED", innerException)
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
        AddData(nameof(ServiceName), serviceName);
        if (statusCode.HasValue)
            AddData(nameof(StatusCode), statusCode);
    }
}
```

---

## Exception Flow by Layer

### 1. Domain Layer

**Throws:**
- DomainException
- ValidationException
- BusinessRuleViolationException

**Example:**
```csharp
public class Receipt : Entity
{
    public void SetTotal(Money total)
    {
        if (total.Amount < 0)
            throw new ValidationException(nameof(Total), "Total cannot be negative");

        if (total.Amount > 1_000_000)
            throw new BusinessRuleViolationException(
                "MaxReceiptTotal",
                "Receipt total cannot exceed $1,000,000");

        Total = total;
    }
}
```

### 2. Service Layer

**Catches:** Domain exceptions
**Wraps/Transforms:** Into application exceptions
**Logs:** All exceptions

**Example:**
```csharp
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _repository;
    private readonly ILogger<ReceiptService> _logger;

    public async Task<ReceiptDto> CreateReceiptAsync(CreateReceiptRequest request)
    {
        try
        {
            // Validate
            if (string.IsNullOrEmpty(request.ImagePath))
                throw new ValidationException(nameof(request.ImagePath), "Image path is required");

            // Create entity
            var receipt = new Receipt(request.Merchant, request.Total);

            // Save
            await _repository.AddAsync(receipt);
            await _repository.UnitOfWork.SaveChangesAsync();

            _logger.LogInformation("Receipt {ReceiptId} created successfully", receipt.Id);

            return MapToDto(receipt);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for receipt creation");
            throw; // Re-throw domain exceptions as-is
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create receipt");
            throw new ApplicationException("Failed to create receipt", "CREATE_RECEIPT_FAILED", ex);
        }
    }
}
```

### 3. Data Layer

**Catches:** EF Core exceptions, database exceptions
**Transforms:** Into data exceptions
**Logs:** All exceptions

**Example:**
```csharp
public class ReceiptRepository : Repository<Receipt>, IReceiptRepository
{
    private readonly ILogger<ReceiptRepository> _logger;

    public override async Task<Receipt?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbContext.Receipts
                .Include(r => r.LineItems)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        catch (SqliteException ex)
        {
            _logger.LogError(ex, "Database error while retrieving receipt {ReceiptId}", id);
            throw new DatabaseException($"Failed to retrieve receipt {id}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in repository");
            throw new RepositoryException(nameof(ReceiptRepository), "GetById", ex);
        }
    }
}
```

### 4. UI Layer

**Catches:** All exceptions
**Displays:** User-friendly messages
**Logs:** All unhandled exceptions

**Example:**
```csharp
public class ReceiptUploadViewModel : ViewModelBase
{
    private readonly IReceiptService _receiptService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ReceiptUploadViewModel> _logger;

    public async Task UploadReceiptAsync()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var result = await _receiptService.ProcessReceiptAsync(ImagePath);

            await _dialogService.ShowSuccessAsync("Receipt uploaded successfully!");
        }
        catch (ValidationException ex)
        {
            ErrorMessage = FormatValidationErrors(ex.Errors);
            _logger.LogWarning(ex, "Validation failed for receipt upload");
        }
        catch (OcrProcessingException ex)
        {
            ErrorMessage = "We couldn't read the receipt. Please try a clearer image.";
            _logger.LogError(ex, "OCR processing failed");
            await _dialogService.ShowErrorAsync(ErrorMessage);
        }
        catch (ReceiptTrackerException ex)
        {
            ErrorMessage = GetUserFriendlyMessage(ex);
            _logger.LogError(ex, "Receipt upload failed");
            await _dialogService.ShowErrorAsync(ErrorMessage);
        }
        catch (Exception ex)
        {
            ErrorMessage = "An unexpected error occurred. Please try again.";
            _logger.LogCritical(ex, "Unexpected error during receipt upload");
            await _dialogService.ShowErrorAsync(ErrorMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private string GetUserFriendlyMessage(ReceiptTrackerException ex)
    {
        return ex.ErrorCode switch
        {
            "OCR_PROCESSING_FAILED" => "Failed to read the receipt. Please try a clearer image.",
            "IMAGE_PROCESSING_FAILED" => "The image format is not supported. Please use JPG or PNG.",
            "DATABASE_ERROR" => "Failed to save the receipt. Please try again.",
            "VALIDATION_FAILED" => "Please check your input and try again.",
            _ => "An error occurred. Please try again."
        };
    }
}
```

---

## Global Exception Handling

### Application-Wide Handler

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Subscribe to unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        base.OnStartup(e);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception occurred");
            ShowCriticalErrorDialog(ex);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled dispatcher exception");
        ShowErrorDialog(e.Exception);
        e.Handled = true; // Prevent app crash
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved(); // Prevent app crash
    }

    private void ShowErrorDialog(Exception ex)
    {
        var message = ex is ReceiptTrackerException rtEx
            ? GetUserFriendlyMessage(rtEx)
            : "An unexpected error occurred. The application will continue.";

        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowCriticalErrorDialog(Exception ex)
    {
        MessageBox.Show(
            "A critical error occurred. The application will now close.\n\n" +
            "Please check the logs for more details.",
            "Critical Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        Environment.Exit(1);
    }
}
```

---

## Logging Strategy for Exceptions

### Log Levels

| Exception Type | Log Level | Include Stack Trace |
|----------------|-----------|---------------------|
| ValidationException | Warning | No |
| BusinessRuleViolationException | Warning | No |
| EntityNotFoundException | Information | No |
| OcrProcessingException | Error | Yes |
| DatabaseException | Error | Yes |
| ExternalServiceException | Error | Yes |
| Unhandled Exception | Critical | Yes |

### Logging Pattern

```csharp
try
{
    // Operation
}
catch (ValidationException ex)
{
    _logger.LogWarning(ex, "Validation failed for {Operation}. Errors: {@Errors}",
        "CreateReceipt", ex.Errors);
    throw;
}
catch (EntityNotFoundException ex)
{
    _logger.LogInformation("Entity not found: {EntityType} {EntityId}",
        ex.EntityType, ex.EntityId);
    throw;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error in {Operation} with parameters {@Parameters}",
        "CreateReceipt", new { request.Merchant, request.Total });
    throw;
}
```

---

## Retry Strategy

### Transient Errors

```csharp
public class RetryPolicy
{
    private readonly ILogger _logger;

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int delayMilliseconds = 1000)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (ExternalServiceException ex) when (IsTransient(ex) && i < maxRetries - 1)
            {
                _logger.LogWarning("Transient error, retrying ({Attempt}/{MaxRetries})",
                    i + 1, maxRetries);
                await Task.Delay(delayMilliseconds * (i + 1)); // Exponential backoff
            }
        }

        throw new ExternalServiceException("Operation failed after retries");
    }

    private bool IsTransient(ExternalServiceException ex)
    {
        return ex.StatusCode is >= 500 or 408 or 429;
    }
}
```

---

## Error Codes Reference

### Domain Errors (1xxx)
- `ENTITY_NOT_FOUND` (1001)
- `DUPLICATE_ENTITY` (1002)
- `VALIDATION_FAILED` (1003)
- `BUSINESS_RULE_VIOLATION` (1004)

### Application Errors (2xxx)
- `OCR_PROCESSING_FAILED` (2001)
- `IMAGE_PROCESSING_FAILED` (2002)
- `EXPORT_FAILED` (2003)
- `IMPORT_FAILED` (2004)

### Data Errors (3xxx)
- `DATABASE_ERROR` (3001)
- `REPOSITORY_OPERATION_FAILED` (3002)
- `FILE_STORAGE_FAILED` (3003)
- `EXTERNAL_SERVICE_FAILED` (3004)

---

## User-Friendly Error Messages

### Error Message Mapping

```csharp
public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        ["ENTITY_NOT_FOUND"] = "The requested item could not be found.",
        ["VALIDATION_FAILED"] = "Please check your input and try again.",
        ["OCR_PROCESSING_FAILED"] = "We couldn't read the receipt. Please try a clearer image.",
        ["DATABASE_ERROR"] = "A problem occurred while saving. Please try again.",
        ["EXTERNAL_SERVICE_FAILED"] = "The service is temporarily unavailable. Please try again later.",
    };

    public static string GetUserMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message)
            ? message
            : "An unexpected error occurred. Please try again.";
    }
}
```

---

## Testing Exception Handling

### Unit Test Example

```csharp
[Fact]
public async Task CreateReceipt_InvalidData_ThrowsValidationException()
{
    // Arrange
    var service = new ReceiptService(_mockRepository.Object, _mockLogger.Object);
    var request = new CreateReceiptRequest { Total = -10 }; // Invalid

    // Act & Assert
    var ex = await Assert.ThrowsAsync<ValidationException>(
        () => service.CreateReceiptAsync(request));

    Assert.Equal("Total cannot be negative", ex.Errors.First().ErrorMessage);
}

[Fact]
public async Task CreateReceipt_RepositoryFails_LogsAndThrows()
{
    // Arrange
    _mockRepository.Setup(r => r.AddAsync(It.IsAny<Receipt>()))
        .ThrowsAsync(new DatabaseException("Connection failed"));

    var service = new ReceiptService(_mockRepository.Object, _mockLogger.Object);

    // Act & Assert
    await Assert.ThrowsAsync<ApplicationException>(
        () => service.CreateReceiptAsync(validRequest));

    _mockLogger.Verify(
        l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
}
```

---

## Summary

### Exception Handling Checklist

- ✅ Use specific exception types
- ✅ Include error codes for tracking
- ✅ Add contextual data to exceptions
- ✅ Log at appropriate levels
- ✅ Translate to user-friendly messages
- ✅ Never swallow exceptions
- ✅ Use global handlers for unhandled exceptions
- ✅ Implement retry for transient errors
- ✅ Test exception scenarios

---

**Next Steps:**
- Implement base exception classes
- Create exception hierarchy in code
- Implement global exception handlers
- Create error message resource file
- Add exception handling to all layers
