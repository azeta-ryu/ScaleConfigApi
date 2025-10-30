namespace ScaleConfigApi.Models;

/// <summary>
/// Input: Represents a single product to be linked to a tag.
/// </summary>
public record ProductTagLink(
    int PluNumber,
    string TagId,
    string ProductName,
    int ImageId
);

// --- NEW MODEL ---
/// <summary>
/// Input: The full request for generating and uploading files to a scale.
/// </summary>
/// <param name="ScaleIpAddress">The IP address of the target scale.</param>
/// <param name="Products">The array of products to link.</param>
public record ScaleUploadRequest(
    string ScaleIpAddress,
    ProductTagLink[] Products
);

/// <summary>
/// Output: The result of the generation, containing a list of files.
/// </summary>
public record ScaleFileGenerationResult(
    List<ScaleFile> Files
);

/// <summary>
/// Output: Represents a single generated configuration file.
/// </summary>
public record ScaleFile(
    string FileName,
    string FileNumberHex,
    int FileNumberDecimal,
    string ContentBase64
);

// --- NEW MODEL ---
/// <summary>
/// Output: The result of the local upload attempt.
/// </summary>
/// <param name="Message">A summary message.</param>
/// <param name="UploadLog">A detailed log of the upload commands.</param>
public record ScaleUploadResult(
    string Message,
    List<string> UploadLog
);