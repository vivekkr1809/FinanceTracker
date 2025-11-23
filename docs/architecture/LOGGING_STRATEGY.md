# Logging Strategy

## Overview
The Receipt Tracker application uses **Serilog** as its logging framework. Serilog provides structured logging capabilities, flexible configuration, and multiple output sinks.

## Logging Framework: Serilog

### Why Serilog?
1. **Structured Logging**: Supports structured log events with properties
2. **Multiple Sinks**: Can write to console, file, database, cloud services, etc.
3. **High Performance**: Efficient and fast logging
4. **Easy Configuration**: Simple fluent API for configuration
5. **Wide Adoption**: Popular in .NET ecosystem with strong community support
6. **Filtering**: Advanced filtering capabilities by source, level, and properties

## Log Levels

The application uses the following log levels according to Serilog standards:

| Level | Usage | Examples |
|-------|-------|----------|
| **Verbose** | Very detailed logs, used only in development | Method entry/exit, detailed variable values |
| **Debug** | Diagnostic information useful for debugging | Query parameters, workflow steps |
| **Information** | General informational messages | Application startup, major operations completed |
| **Warning** | Indication that something unexpected happened | Deprecated API usage, recoverable errors |
| **Error** | Error events that might still allow the app to continue | Failed OCR processing, database connection errors |
| **Fatal** | Critical errors that cause application shutdown | Unrecoverable exceptions, data corruption |

## Log Output Configuration

### Development Environment
- **Minimum Level**: Debug
- **Sinks**:
  - Console (with colored output)
  - File (daily rolling, 30-day retention)
  - Debug output (Visual Studio)

### Production Environment
- **Minimum Level**: Information
- **Sinks**:
  - File (daily rolling, 30-day retention)
  - Console (for service monitoring)

### Log File Location
- **Path**: `logs/receipt-tracker-{Date}.txt`
- **Format**: `receipt-tracker-2025-11-23.txt`
- **Retention**: 30 days
- **Rolling**: Daily at midnight

## Logging Best Practices

### 1. Use Structured Logging
```csharp
// Good - Structured logging with properties
_logger.LogInformation("Receipt processed successfully. ReceiptId: {ReceiptId}, Total: {Total}",
    receiptId, total);

// Bad - String concatenation
_logger.LogInformation($"Receipt processed successfully. ReceiptId: {receiptId}, Total: {total}");
```

### 2. Use Appropriate Log Levels
```csharp
// Information - Normal operation
_logger.LogInformation("Receipt {ReceiptId} uploaded successfully", receiptId);

// Warning - Something unexpected but recoverable
_logger.LogWarning("OCR confidence low ({Confidence}%) for receipt {ReceiptId}",
    confidence, receiptId);

// Error - Operation failed but application continues
_logger.LogError(exception, "Failed to process receipt {ReceiptId}", receiptId);

// Fatal - Critical failure
_logger.LogCritical(exception, "Database connection lost and cannot be recovered");
```

### 3. Include Context
Always include relevant context in log messages:
- Entity IDs (ReceiptId, CategoryId, etc.)
- Operation being performed
- User information (if applicable)
- Transaction/correlation IDs for tracking

### 4. Don't Log Sensitive Information
Never log:
- Personal Identifiable Information (PII) beyond IDs
- Credit card numbers or financial account details
- Passwords or API keys
- Full receipt images or sensitive document content

```csharp
// Good
_logger.LogInformation("Processing receipt for merchant {MerchantName}", merchantName);

// Bad - Don't log sensitive data
_logger.LogInformation("Credit card ending in {CardNumber}", fullCardNumber);
```

### 5. Use Scopes for Related Operations
```csharp
using (_logger.BeginScope("Processing receipt {ReceiptId}", receiptId))
{
    _logger.LogInformation("Starting OCR extraction");
    _logger.LogInformation("Parsing merchant name");
    _logger.LogInformation("Extracting line items");
}
```

### 6. Log Exceptions Properly
```csharp
try
{
    // Operation
}
catch (Exception ex)
{
    // Good - Exception as first parameter, message second
    _logger.LogError(ex, "Failed to save receipt {ReceiptId}", receiptId);

    // Bad - Exception in message
    _logger.LogError("Error: " + ex.ToString());
}
```

## Log Message Templates

### Standard Templates

#### Application Lifecycle
```csharp
_logger.LogInformation("Application starting...");
_logger.LogInformation("Application ready");
_logger.LogInformation("Application shutting down...");
```

#### Data Operations
```csharp
_logger.LogInformation("Creating {EntityType} with ID {EntityId}", entityType, entityId);
_logger.LogInformation("Updating {EntityType} {EntityId}", entityType, entityId);
_logger.LogInformation("Deleting {EntityType} {EntityId}", entityType, entityId);
_logger.LogInformation("Retrieved {Count} {EntityType} records", count, entityType);
```

#### Receipt Processing
```csharp
_logger.LogInformation("Starting OCR processing for receipt {ReceiptId}", receiptId);
_logger.LogInformation("OCR completed for receipt {ReceiptId}. Confidence: {Confidence}%", receiptId, confidence);
_logger.LogWarning("Low confidence OCR result for receipt {ReceiptId}. Manual review recommended", receiptId);
_logger.LogError(ex, "OCR processing failed for receipt {ReceiptId}", receiptId);
```

#### Performance Monitoring
```csharp
_logger.LogInformation("Operation {OperationName} completed in {ElapsedMs}ms", operationName, elapsed);
_logger.LogWarning("Slow query detected. Query: {QueryName}, Duration: {ElapsedMs}ms", queryName, elapsed);
```

## Integration with Dependency Injection

Logging is integrated with Microsoft.Extensions.Logging and available via dependency injection:

```csharp
public class ExpenseService
{
    private readonly ILogger<ExpenseService> _logger;

    public ExpenseService(ILogger<ExpenseService> logger)
    {
        _logger = logger;
    }

    public async Task ProcessReceiptAsync(int receiptId)
    {
        _logger.LogInformation("Processing receipt {ReceiptId}", receiptId);
        // Implementation
    }
}
```

## Log Analysis and Monitoring

### Local Development
- Logs are written to `logs/` directory
- Use any text editor to view log files
- Console output available during debugging

### Production Monitoring
- Consider integrating with log aggregation services (future enhancement):
  - Seq (https://datalust.co/seq)
  - ELK Stack (Elasticsearch, Logstash, Kibana)
  - Azure Application Insights
  - Sentry

## Configuration

### Changing Log Levels at Runtime
Future enhancement: Support for appsettings.json configuration:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "ReceiptTracker": "Debug"
      }
    }
  }
}
```

## Troubleshooting

### No Logs Appearing
1. Check that `logs/` directory is writable
2. Verify Serilog is configured in `Program.cs` or `App.xaml.cs`
3. Check minimum log level setting

### Log Files Growing Too Large
1. Verify daily rolling is configured
2. Check retention policy (default: 30 days)
3. Consider reducing log level in production

### Performance Issues
1. Avoid logging in tight loops
2. Use appropriate log levels
3. Consider async logging for high-throughput scenarios

---

**Last Updated:** 2025-11-23
**Document Version:** 1.0
