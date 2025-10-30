namespace ScaleConfigApi;

internal static partial class Log
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "API request received to generate configs for {ProductCount} products.")]
    public static partial void ApiRequestReceived(ILogger logger, int productCount);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Generated file {FileName} ({FileNumberHex}) with {ByteCount} bytes.")]
    public static partial void FileGenerationComplete(ILogger logger, string fileName, string fileNumberHex, int byteCount);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Generating MOCK file {FileName}. The file structure is not defined in the documentation.")]
    public static partial void MockFileGenerated(ILogger logger, string fileName);

    // --- NEW LOGS ---
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Information,
        Message = "API request received to UPLOAD configs for {ProductCount} products to {ScaleIpAddress}.")]
    public static partial void UploadRequestReceived(ILogger logger, int productCount, string scaleIpAddress);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "Wrote temporary file: {FilePath}")]
    public static partial void FileWritten(ILogger logger, string filePath);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "Executing: {ProcessPath} {Arguments}")]
    public static partial void ExecutingProcess(ILogger logger, string processPath, string arguments);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Warning,
        Message = "Process exited with code {ExitCode}. Output: {Output}")]
    public static partial void ProcessError(ILogger logger, int exitCode, string output);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Warning,
        Message = "Failed to clean up file: {FilePath}. Error: {ErrorMessage}")]
    public static partial void FileCleanupFailed(ILogger logger, string filePath, string errorMessage, Exception ex);

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred: {ErrorMessage}")]
    public static partial void ApiError(ILogger logger, string errorMessage, Exception ex);

    // --- NEW LOGS ---
    [LoggerMessage(
        EventId = 5002,
        Level = LogLevel.Error,
        Message = "Upload failed. Invalid IP Address: {ScaleIpAddress}")]
    public static partial void UploadFailedInvalidIp(ILogger logger, string scaleIpAddress);

    [LoggerMessage(
        EventId = 5003,
        Level = LogLevel.Error,
        Message = "Upload failed. Driver not found at: {DriverPath}")]
    public static partial void UploadFailedNoDriver(ILogger logger, string driverPath);

    [LoggerMessage(
        EventId = 5004,
        Level = LogLevel.Error,
        Message = "Upload process failed: {ErrorMessage}")]
    public static partial void UploadFailed(ILogger logger, string errorMessage, Exception ex);
}