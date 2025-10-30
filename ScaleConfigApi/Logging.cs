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

    [LoggerMessage(
        EventId = 5001,
        Level = LogLevel.Error,
        Message = "An unhandled exception occurred: {ErrorMessage}")]
    public static partial void ApiError(ILogger logger, string errorMessage, Exception ex);
}