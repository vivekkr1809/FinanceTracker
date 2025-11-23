# Receipt Processing Pipeline

## Receipt Tracker - Data Flow Documentation

**Version:** 1.0
**Last Updated:** 2025-11-23
**Phase:** 1.2 - Architecture Design

---

## Overview

This document describes the complete data flow for receipt processing in the Receipt Tracker application, from image upload through OCR processing, data extraction, validation, and persistence.

---

## High-Level Pipeline Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                   Receipt Processing Pipeline                    │
└─────────────────────────────────────────────────────────────────┘

User Upload → Validation → Image Processing → OCR → Data Extraction
     → Categorization → Entity Creation → Persistence → UI Update
```

---

## Detailed Pipeline Flow

### Stage 1: Image Upload

```
User Action (UI Layer)
    │
    ├─ Drag & Drop File
    ├─ File Browser Select
    ├─ Paste from Clipboard
    └─ Camera Capture
    │
    ▼
ReceiptUploadViewModel
    │
    ├─ Validate File Exists
    ├─ Check File Extension
    ├─ Check File Size
    └─ Update UI State (Loading)
    │
    ▼
Call ReceiptService.ProcessReceiptAsync(imagePath)
```

#### Code Example: ViewModel

```csharp
public class ReceiptUploadViewModel : ViewModelBase
{
    private readonly IReceiptService _receiptService;
    private readonly IDialogService _dialogService;

    public async Task UploadReceiptAsync(string filePath)
    {
        try
        {
            // 1. UI Validation
            if (!File.Exists(filePath))
            {
                await _dialogService.ShowErrorAsync("File not found");
                return;
            }

            var extension = Path.GetExtension(filePath).ToLower();
            if (!IsValidImageExtension(extension))
            {
                await _dialogService.ShowErrorAsync("Unsupported file format");
                return;
            }

            // 2. Show progress
            IsBusy = true;
            UploadProgress = 0;

            // 3. Call service
            var result = await _receiptService.ProcessReceiptAsync(
                filePath,
                new Progress<int>(p => UploadProgress = p));

            // 4. Navigate to detail view
            await NavigateToReceiptDetail(result.Id);
        }
        catch (Exception ex)
        {
            await HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

---

### Stage 2: Service Layer Entry Point

```
ReceiptService.ProcessReceiptAsync()
    │
    ├─ Log: "Starting receipt processing"
    ├─ Validate Input Parameters
    ├─ Check File Size Against Config
    └─ Begin Transaction
    │
    ▼
Create Processing Context
```

#### Code Example: Service

```csharp
public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly IOcrService _ocrService;
    private readonly IImageProcessor _imageProcessor;
    private readonly ICategoryService _categoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReceiptService> _logger;
    private readonly AppSettings _settings;

    public async Task<ReceiptDto> ProcessReceiptAsync(
        string imagePath,
        IProgress<int>? progress = null)
    {
        using var scope = _logger.BeginScope("ProcessReceipt {ImagePath}", imagePath);

        try
        {
            _logger.LogInformation("Starting receipt processing");

            // Create processing context
            var context = new ReceiptProcessingContext
            {
                OriginalImagePath = imagePath,
                ProcessingId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow
            };

            // Execute pipeline
            progress?.Report(10);
            await ValidateImageAsync(context);

            progress?.Report(20);
            await SaveImageAsync(context);

            progress?.Report(40);
            await ProcessOcrAsync(context);

            progress?.Report(60);
            await ExtractDataAsync(context);

            progress?.Report(70);
            await CategorizeAsync(context);

            progress?.Report(80);
            var receipt = await CreateReceiptEntityAsync(context);

            progress?.Report(90);
            await SaveReceiptAsync(receipt);

            progress?.Report(100);

            _logger.LogInformation("Receipt {ReceiptId} processed successfully in {Duration}ms",
                receipt.Id, (DateTime.UtcNow - context.StartTime).TotalMilliseconds);

            return MapToDto(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Receipt processing failed");
            throw new ApplicationException("Failed to process receipt", "RECEIPT_PROCESSING_FAILED", ex);
        }
    }
}
```

---

### Stage 3: Image Validation

```
ValidateImageAsync()
    │
    ├─ Check File Exists
    ├─ Check File Size (Max from Config)
    ├─ Check File Format (JPEG, PNG, HEIC, PDF)
    ├─ Check Image Dimensions
    └─ Virus Scan (Optional, Future)
    │
    ▼
Validation Result
```

#### Validation Rules

| Validation | Limit | Action on Fail |
|------------|-------|----------------|
| File Exists | Required | Throw ValidationException |
| File Size | Max 10 MB (configurable) | Throw ValidationException |
| Format | JPG, PNG, HEIC, PDF | Throw ValidationException |
| Dimensions | Max 4000x4000 | Auto-resize |
| Corruption | Valid image | Throw ImageProcessingException |

#### Code Example

```csharp
private async Task ValidateImageAsync(ReceiptProcessingContext context)
{
    var fileInfo = new FileInfo(context.OriginalImagePath);

    // Check exists
    if (!fileInfo.Exists)
        throw new ValidationException(nameof(context.OriginalImagePath), "File does not exist");

    // Check size
    var maxSize = _settings.MaxImageSizeMB * 1024 * 1024;
    if (fileInfo.Length > maxSize)
        throw new ValidationException(
            nameof(context.OriginalImagePath),
            $"File size ({fileInfo.Length / 1024 / 1024}MB) exceeds maximum ({_settings.MaxImageSizeMB}MB)");

    // Check format
    var extension = fileInfo.Extension.ToLowerInvariant();
    var supportedFormats = new[] { ".jpg", ".jpeg", ".png", ".heic", ".pdf" };
    if (!supportedFormats.Contains(extension))
        throw new ValidationException(
            nameof(context.OriginalImagePath),
            $"Unsupported file format: {extension}");

    // Check image can be loaded
    try
    {
        await _imageProcessor.ValidateImageAsync(context.OriginalImagePath);
    }
    catch (Exception ex)
    {
        throw new ImageProcessingException(
            context.OriginalImagePath,
            "Image is corrupted or invalid",
            ex);
    }

    _logger.LogDebug("Image validation successful");
}
```

---

### Stage 4: Image Storage

```
SaveImageAsync()
    │
    ├─ Generate Unique Filename (GUID)
    ├─ Compress/Optimize Image
    ├─ Generate Thumbnail
    ├─ Copy to Storage Directory
    ├─ Calculate File Hash (for deduplication)
    └─ Update Context with Paths
    │
    ▼
Stored Image Paths
```

#### Storage Structure

```
images/
├── originals/
│   └── {year}/
│       └── {month}/
│           └── {guid}.jpg
└── thumbnails/
    └── {year}/
        └── {month}/
            └── {guid}_thumb.jpg
```

#### Code Example

```csharp
private async Task SaveImageAsync(ReceiptProcessingContext context)
{
    try
    {
        // Generate paths
        var now = DateTime.UtcNow;
        var filename = $"{context.ProcessingId}.jpg";
        var relativePath = Path.Combine(now.Year.ToString(), now.Month.ToString("D2"), filename);
        var fullPath = Path.Combine(_settings.GetFullImageStoragePath(), "originals", relativePath);
        var thumbnailPath = Path.Combine(_settings.GetFullImageStoragePath(), "thumbnails", relativePath);

        // Ensure directories exist
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        Directory.CreateDirectory(Path.GetDirectoryName(thumbnailPath)!);

        // Process and save image
        await _imageProcessor.ProcessAndSaveAsync(
            context.OriginalImagePath,
            fullPath,
            thumbnailPath);

        // Calculate hash for deduplication
        context.ImageHash = await _fileStorage.CalculateHashAsync(fullPath);

        // Update context
        context.StoredImagePath = fullPath;
        context.ThumbnailPath = thumbnailPath;
        context.RelativeImagePath = relativePath;

        _logger.LogDebug("Image saved to {Path}", fullPath);
    }
    catch (Exception ex)
    {
        throw new FileStorageException(context.OriginalImagePath, "Save", ex);
    }
}
```

---

### Stage 5: OCR Processing

```
ProcessOcrAsync()
    │
    ├─ Select OCR Provider (from Config)
    ├─ Check for Cached Result
    ├─ Call OCR API/Library
    ├─ Retry on Transient Failures
    ├─ Fallback to Offline if Cloud Fails
    ├─ Cache Result
    └─ Update Context with OCR Text
    │
    ▼
Raw OCR Text & Metadata
```

#### OCR Provider Selection

```
Configuration.DefaultProvider
    │
    ├─ Azure → AzureOcrProvider
    ├─ Tesseract → TesseractOcrProvider
    └─ Windows → WindowsOcrProvider
    │
    ▼
If Cloud Fails & FallbackEnabled
    │
    └─ Use Tesseract (Offline)
```

#### Code Example

```csharp
private async Task ProcessOcrAsync(ReceiptProcessingContext context)
{
    try
    {
        var ocrSettings = _settings.GetOcrSettings();

        // Check cache first
        var cacheKey = $"ocr_{context.ImageHash}";
        if (ocrSettings.EnableCaching)
        {
            var cachedResult = await _cache.GetAsync<OcrResult>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Using cached OCR result");
                context.OcrResult = cachedResult;
                return;
            }
        }

        // Get OCR provider
        var provider = _ocrProviderFactory.GetProvider(ocrSettings.DefaultProvider);

        // Process with retry
        var retryPolicy = Policy
            .Handle<ExternalServiceException>()
            .WaitAndRetryAsync(
                ocrSettings.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        OcrResult result;
        try
        {
            result = await retryPolicy.ExecuteAsync(() =>
                provider.ProcessImageAsync(context.StoredImagePath));
        }
        catch (Exception ex) when (ocrSettings.FallbackToOffline)
        {
            _logger.LogWarning(ex, "Cloud OCR failed, falling back to Tesseract");
            var fallbackProvider = _ocrProviderFactory.GetProvider(OcrProviderType.Tesseract);
            result = await fallbackProvider.ProcessImageAsync(context.StoredImagePath);
        }

        // Cache result
        if (ocrSettings.EnableCaching)
        {
            await _cache.SetAsync(cacheKey, result,
                TimeSpan.FromMinutes(ocrSettings.CacheDurationMinutes));
        }

        context.OcrResult = result;

        _logger.LogInformation("OCR completed. Confidence: {Confidence}%, Provider: {Provider}",
            result.AverageConfidence, result.ProviderName);
    }
    catch (Exception ex)
    {
        throw new OcrProcessingException(context.StoredImagePath, "OCR Provider", ex);
    }
}
```

---

### Stage 6: Data Extraction

```
ExtractDataAsync()
    │
    ├─ Parse Merchant Name
    ├─ Extract Transaction Date
    ├─ Find Total Amount
    ├─ Find Tax Amount
    ├─ Extract Line Items
    ├─ Find Payment Method
    ├─ Confidence Scoring
    └─ Update Context with Extracted Data
    │
    ▼
Structured Receipt Data
```

#### Extraction Strategies

**Merchant Name:**
- Look for text at top of receipt
- Match against known merchant patterns
- Use address/phone to confirm

**Date:**
- Regex patterns: MM/DD/YYYY, DD-MM-YYYY, etc.
- Date keywords: "Date:", "Trans Date:", etc.
- Validate date is reasonable (not future, not too old)

**Total:**
- Keywords: "Total", "Amount Due", "Grand Total"
- Look for largest monetary value
- Must be after line items

**Tax:**
- Keywords: "Tax", "GST", "VAT", "Sales Tax"
- Usually before total

**Line Items:**
- Text between header and total
- Pattern: Description + Quantity + Price
- Skip subtotals and other labels

#### Code Example

```csharp
private async Task ExtractDataAsync(ReceiptProcessingContext context)
{
    var extractor = new ReceiptDataExtractor(_logger);
    var ocrText = context.OcrResult.Text;

    // Extract merchant
    context.ExtractedData.Merchant = extractor.ExtractMerchant(ocrText);
    context.ExtractedData.MerchantConfidence = extractor.LastConfidenceScore;

    // Extract date
    context.ExtractedData.TransactionDate = extractor.ExtractDate(ocrText);
    context.ExtractedData.DateConfidence = extractor.LastConfidenceScore;

    // Extract amounts
    var amounts = extractor.ExtractAmounts(ocrText);
    context.ExtractedData.Total = amounts.Total;
    context.ExtractedData.TotalConfidence = amounts.TotalConfidence;
    context.ExtractedData.Tax = amounts.Tax;
    context.ExtractedData.TaxConfidence = amounts.TaxConfidence;
    context.ExtractedData.Subtotal = amounts.Subtotal;

    // Extract line items
    context.ExtractedData.LineItems = extractor.ExtractLineItems(ocrText);

    // Extract payment method (if visible on receipt)
    context.ExtractedData.PaymentMethod = extractor.ExtractPaymentMethod(ocrText);

    // Overall confidence
    context.ExtractedData.OverallConfidence = CalculateOverallConfidence(context.ExtractedData);

    _logger.LogInformation("Data extraction completed. Overall confidence: {Confidence}%",
        context.ExtractedData.OverallConfidence);

    // Flag for manual review if confidence is low
    if (context.ExtractedData.OverallConfidence < _settings.MinConfidenceLevel)
    {
        context.RequiresManualReview = true;
        _logger.LogWarning("Low confidence extraction, flagging for manual review");
    }
}
```

---

### Stage 7: Auto-Categorization

```
CategorizeAsync()
    │
    ├─ Get User Settings (Auto-Categorize Enabled?)
    ├─ Match Merchant to Known Categories
    ├─ Use ML Model (Future)
    ├─ Check User's Historical Categories for Merchant
    ├─ Apply Rule-Based Logic
    └─ Assign Category (or default "Uncategorized")
    │
    ▼
Category Assignment
```

#### Categorization Logic

```
1. Check exact merchant match
   └─ Starbucks → Food & Dining → Coffee Shops

2. Check merchant pattern
   └─ Contains "gas" or "fuel" → Transportation → Gas

3. Check user history
   └─ User always categorizes "Walmart" as "Groceries"

4. Check amount ranges
   └─ Very small amounts (<$5) → Food & Dining → Snacks

5. Default
   └─ Uncategorized
```

#### Code Example

```csharp
private async Task CategorizeAsync(ReceiptProcessingContext context)
{
    var userSettings = await _userSettingsService.GetUserSettingsAsync();

    if (!userSettings.AutoCategorize)
    {
        context.ExtractedData.CategoryId = userSettings.DefaultCategoryId;
        return;
    }

    var merchant = context.ExtractedData.Merchant;

    // 1. Exact merchant match
    var category = await _categoryService.GetCategoryByMerchantAsync(merchant);
    if (category != null)
    {
        context.ExtractedData.CategoryId = category.Id;
        context.ExtractedData.CategoryConfidence = 100;
        return;
    }

    // 2. User history
    category = await _categoryService.GetUserPreferredCategoryAsync(merchant);
    if (category != null)
    {
        context.ExtractedData.CategoryId = category.Id;
        context.ExtractedData.CategoryConfidence = 90;
        return;
    }

    // 3. Pattern matching
    category = await _categoryService.MatchCategoryByPatternAsync(merchant);
    if (category != null)
    {
        context.ExtractedData.CategoryId = category.Id;
        context.ExtractedData.CategoryConfidence = 75;
        return;
    }

    // 4. Default
    context.ExtractedData.CategoryId = await _categoryService.GetUncategorizedCategoryIdAsync();
    context.ExtractedData.CategoryConfidence = 0;

    _logger.LogInformation("Categorized as {CategoryId} with {Confidence}% confidence",
        context.ExtractedData.CategoryId, context.ExtractedData.CategoryConfidence);
}
```

---

### Stage 8: Entity Creation

```
CreateReceiptEntityAsync()
    │
    ├─ Create Receipt Entity
    ├─ Set Properties from Extracted Data
    ├─ Create LineItem Entities
    ├─ Validate Entity
    ├─ Check for Duplicates
    └─ Return Receipt Entity
    │
    ▼
Receipt Domain Entity
```

#### Code Example

```csharp
private async Task<Receipt> CreateReceiptEntityAsync(ReceiptProcessingContext context)
{
    var data = context.ExtractedData;

    // Create money value objects
    var total = new Money(data.Total ?? 0, data.Currency ?? "USD");
    var tax = data.Tax.HasValue ? new Money(data.Tax.Value, data.Currency ?? "USD") : null;

    // Create receipt entity
    var receipt = new Receipt
    {
        Merchant = data.Merchant ?? "Unknown",
        TransactionDate = data.TransactionDate ?? DateTime.UtcNow,
        Total = total,
        Tax = tax,
        CategoryId = data.CategoryId,
        ImagePath = context.RelativeImagePath,
        ThumbnailPath = Path.GetFileName(context.ThumbnailPath),
        OcrText = context.OcrResult.Text,
        OcrConfidence = context.ExtractedData.OverallConfidence,
        RequiresReview = context.RequiresManualReview,
        ProcessingId = context.ProcessingId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    // Add line items
    foreach (var item in data.LineItems)
    {
        receipt.AddLineItem(new LineItem
        {
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = new Money(item.UnitPrice, data.Currency ?? "USD"),
            Total = new Money(item.Total, data.Currency ?? "USD")
        });
    }

    // Validate
    receipt.Validate();

    // Check for duplicates
    var duplicate = await _receiptRepository.FindDuplicateAsync(
        receipt.Merchant,
        receipt.TransactionDate,
        receipt.Total);

    if (duplicate != null)
    {
        _logger.LogWarning("Potential duplicate receipt found: {DuplicateId}", duplicate.Id);
        receipt.MarkAsPotentialDuplicate(duplicate.Id);
    }

    return receipt;
}
```

---

### Stage 9: Persistence

```
SaveReceiptAsync()
    │
    ├─ Begin Transaction
    ├─ Add Receipt to Repository
    ├─ Save Line Items
    ├─ Update Category Usage Stats
    ├─ Create Audit Log Entry
    ├─ Commit Transaction
    └─ Return Saved Receipt
    │
    ▼
Persisted Receipt (with ID)
```

#### Code Example

```csharp
private async Task SaveReceiptAsync(Receipt receipt)
{
    try
    {
        // Add to repository
        await _receiptRepository.AddAsync(receipt);

        // Update category usage
        if (receipt.CategoryId.HasValue)
        {
            await _categoryService.IncrementUsageCountAsync(receipt.CategoryId.Value);
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Receipt {ReceiptId} saved successfully", receipt.Id);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to save receipt");
        throw new RepositoryException(nameof(ReceiptRepository), "Add", ex);
    }
}
```

---

### Stage 10: UI Update

```
Service Returns ReceiptDto
    │
    ▼
ViewModel Receives Result
    │
    ├─ Update ReceiptList (Observable Collection)
    ├─ Navigate to Detail View
    ├─ Show Success Notification
    └─ Clear Upload State
    │
    ▼
UI Updated
```

---

## Processing Context Data Structure

```csharp
public class ReceiptProcessingContext
{
    public Guid ProcessingId { get; set; }
    public DateTime StartTime { get; set; }

    // Image Info
    public string OriginalImagePath { get; set; }
    public string StoredImagePath { get; set; }
    public string ThumbnailPath { get; set; }
    public string RelativeImagePath { get; set; }
    public string ImageHash { get; set; }

    // OCR Results
    public OcrResult OcrResult { get; set; }

    // Extracted Data
    public ExtractedReceiptData ExtractedData { get; set; } = new();

    // Flags
    public bool RequiresManualReview { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

public class ExtractedReceiptData
{
    public string? Merchant { get; set; }
    public int MerchantConfidence { get; set; }

    public DateTime? TransactionDate { get; set; }
    public int DateConfidence { get; set; }

    public decimal? Total { get; set; }
    public int TotalConfidence { get; set; }

    public decimal? Tax { get; set; }
    public int TaxConfidence { get; set; }

    public decimal? Subtotal { get; set; }

    public string? Currency { get; set; } = "USD";

    public int? CategoryId { get; set; }
    public int CategoryConfidence { get; set; }

    public string? PaymentMethod { get; set; }

    public List<ExtractedLineItem> LineItems { get; set; } = new();

    public int OverallConfidence { get; set; }
}
```

---

## Error Handling in Pipeline

### Stage-Specific Error Handling

```
Each Stage:
    try {
        // Stage logic
    }
    catch (ValidationException) {
        // Log warning
        // Show user-friendly message
        // Stop pipeline
    }
    catch (ExternalServiceException) {
        // Log error
        // Retry if appropriate
        // Fallback if available
    }
    catch (Exception) {
        // Log critical error
        // Rollback transaction
        // Clean up temporary files
        // Show error to user
    }
```

---

## Performance Optimization

### Async Throughout

```csharp
public async Task ProcessReceiptAsync(...)
{
    await ValidateImageAsync(...);      // I/O
    await SaveImageAsync(...);          // I/O
    await ProcessOcrAsync(...);         // Network/CPU
    await ExtractDataAsync(...);        // CPU
    await CategorizeAsync(...);         // Database
    await SaveReceiptAsync(...);        // Database
}
```

### Parallel Processing (Future)

```csharp
// Process multiple receipts in parallel
await Parallel.ForEachAsync(imagePaths, async (path, ct) =>
{
    await ProcessReceiptAsync(path);
});
```

### Caching

- OCR results cached by image hash
- Category mappings cached
- User settings cached

---

## Monitoring and Metrics

### Key Metrics to Track

- Processing time per stage
- Overall processing time
- OCR confidence scores
- Auto-categorization accuracy
- Error rates by stage
- Duplicate detection rate

### Logging

```csharp
_logger.LogInformation("Receipt processing started. ProcessingId: {ProcessingId}", context.ProcessingId);
_logger.LogDebug("Stage: Image validation completed");
_logger.LogDebug("Stage: Image saved. Size: {Size}KB", fileSize);
_logger.LogInformation("Stage: OCR completed. Confidence: {Confidence}%", confidence);
_logger.LogDebug("Stage: Data extraction completed");
_logger.LogInformation("Receipt processing completed in {Duration}ms", duration);
```

---

## Manual Review Workflow

When `RequiresManualReview = true`:

```
Receipt Saved with ReviewRequired flag
    │
    ▼
User notified in UI
    │
    ▼
User opens Receipt Detail View
    │
    ├─ Shows OCR text
    ├─ Shows extracted data
    ├─ Highlights low-confidence fields
    └─ Allows editing
    │
    ▼
User corrects data
    │
    ▼
Save → Mark as Reviewed
```

---

## Future Enhancements

### Phase 1 (Current)
- Basic OCR with single provider
- Simple pattern-based extraction
- Manual categorization

### Phase 2 (Future)
- Multiple OCR providers with voting
- Machine learning for extraction
- ML-based categorization
- Batch processing
- Email receipt forwarding

### Phase 3 (Future)
- Receipt template matching
- Merchant-specific extraction logic
- Smart duplicate detection
- Auto-learn from corrections

---

## Summary: Complete Flow

```
User Upload
    ↓
[UI Layer] ReceiptUploadViewModel
    ↓
[Service Layer] ReceiptService.ProcessReceiptAsync()
    ↓
[Stage 1] ValidateImageAsync() → Validate file
    ↓
[Stage 2] SaveImageAsync() → Store & compress
    ↓
[Stage 3] ProcessOcrAsync() → Extract text
    ↓
[Stage 4] ExtractDataAsync() → Parse structured data
    ↓
[Stage 5] CategorizeAsync() → Assign category
    ↓
[Stage 6] CreateReceiptEntityAsync() → Create domain entity
    ↓
[Stage 7] SaveReceiptAsync() → Persist to database
    ↓
[UI Layer] Update UI with result
    ↓
Success!
```

---

**Next Steps:**
- Implement ReceiptProcessingContext class
- Implement each pipeline stage
- Add comprehensive logging
- Add telemetry and metrics
- Create unit tests for each stage
