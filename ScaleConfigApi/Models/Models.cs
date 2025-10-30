namespace ScaleConfigApi.Models;

/// <summary>
/// Input: Represents a single product to be linked to a tag.
/// </summary>
/// <param name="PluNumber">The PLU number (e.g., 10).</param>
/// <param name="TagId">The 9-digit physical tag ID (e.g., "111111111").</param>
/// <param name="ProductName">A human-readable name (e.g., "Angus Beef").</param>
/// <param name="ImageId">The image number on the scale (e.g., 42).</param>
public record ProductTagLink(
    int PluNumber,
    string TagId,
    string ProductName,
    int ImageId
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
/// <param name="FileName">Descriptive name (e.g., "PLU4 FILE").</param>
/// <param name="FileNumberHex">The hex file ID (e.g., "AAH").</param>
/// <param name="FileNumberDecimal">The decimal file ID (e.g., 170).</param>
/// <param name="ContentBase64">The binary file content, encoded as a Base64 string.</param>
public record ScaleFile(
    string FileName,
    string FileNumberHex,
    int FileNumberDecimal,
    string ContentBase64
);