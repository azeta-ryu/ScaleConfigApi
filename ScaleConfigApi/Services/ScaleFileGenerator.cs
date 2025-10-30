using System.Text;
using ScaleConfigApi.Models;

namespace ScaleConfigApi.Services;

/// <summary>
/// Internal model for passing raw file data between services.
/// </summary>
public record GeneratedFileData(
    string FileName,
    string FileNumberHex,
    int FileNumberDecimal,
    byte[] Content
);

/// <summary>
/// Generates the binary configuration files for the DIGI scale.
/// </summary>
public class ScaleFileGenerator(ILogger<ScaleFileGenerator> logger)
{
    private readonly ILogger<ScaleFileGenerator> _logger = logger;

    /// <summary>
    /// Generates all required config files as raw byte arrays.
    /// This is used by the new local uploader service.
    /// </summary>
    public List<GeneratedFileData> GenerateFilesRaw(ProductTagLink[] products)
    {
        return
        [
            GenerateAahFile(products),
            GenerateCchFile(products),
            Generate25hFileMock() // The spec for 25H is not provided
        ];
    }

    /// <summary>
    /// Generates files and converts them to the Base64 response.
    /// This is used by the original /generate-scale-configs endpoint.
    /// </summary>
    public ScaleFileGenerationResult GenerateFilesForApiResponse(ProductTagLink[] products)
    {
        var rawFiles = GenerateFilesRaw(products);

        var apiFiles = rawFiles.Select(file => new ScaleFile(
            file.FileName,
            file.FileNumberHex,
            file.FileNumberDecimal,
            Convert.ToBase64String(file.Content)
        )).ToList();

        return new ScaleFileGenerationResult(Files: apiFiles);
    }

    /// <summary>
    /// Generates the AA PLU4 FILE (AAH)
    /// This file links supplementary e-Label data (like images) to the PLUs.
    /// </summary>
    private GeneratedFileData GenerateAahFile(ProductTagLink[] products)
    {
        using var ms = new MemoryStream();
        // ... (The BinaryWriter logic is identical to your previous version) ...
        using (var writer = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            foreach (var product in products)
            {
                short recordSize = 30;
                BinaryWriterHelper.WriteBcd(writer, product.PluNumber, 4);
                writer.Write(recordSize);
                int status = 1 << 5;
                writer.Write(status);
                writer.Write(0);
                BinaryWriterHelper.WriteBcd(writer, 0, 2);
                BinaryWriterHelper.WriteBcd(writer, product.ImageId, 4);
                BinaryWriterHelper.WriteBcd(writer, 0, 4);
                BinaryWriterHelper.WriteBcd(writer, 0, 4);
                BinaryWriterHelper.WriteBcd(writer, 0, 4);
                BinaryWriterHelper.WriteBcd(writer, 0, 4);
            }
        }

        var bytes = ms.ToArray();
        Log.FileGenerationComplete(_logger, "AA PLU4 FILE", "AAH", bytes.Length);

        return new GeneratedFileData(
            "AA PLU4 FILE",
            "AAH",
            170, //
            bytes
        );
    }

    /// <summary>
    /// Generates the CC TAG INFO FILE (CCH)
    /// This file creates the link between a PLU and a physical 9-digit Tag ID.
    /// </summary>
    private GeneratedFileData GenerateCchFile(ProductTagLink[] products)
    {
        using var ms = new MemoryStream();
        // ... (The BinaryWriter logic is identical to your previous version) ...
        using (var writer = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            foreach (var product in products)
            {
                short recordSize = 176;
                writer.Write(int.Parse(product.TagId));
                writer.Write(recordSize);
                BinaryWriterHelper.WritePaddedString(writer, product.ProductName, 100);
                writer.Write(999);
                writer.Write(product.PluNumber);
                for (int i = 0; i < 17; i++)
                {
                    writer.Write(0);
                }
            }
        }

        var bytes = ms.ToArray();
        Log.FileGenerationComplete(_logger, "CC TAG INFO FILE", "CCH", bytes.Length);

        return new GeneratedFileData(
            "CC TAG INFO FILE",
            "CCH",
            204, //
            bytes
        );
    }

    /// <summary>
    /// Generates a MOCK PLU FILE (25H).
    /// </summary>
    private GeneratedFileData Generate25hFileMock()
    {
        Log.MockFileGenerated(_logger, "PLU FILE (25H)");
        string mockContent =
            "// MOCK FILE - The structure for 25H (PLU FILE) was not " +
            "// provided in the setup documentation. " +
            "// This file is required to trigger the tag update.";

        var bytes = Encoding.ASCII.GetBytes(mockContent);

        return new GeneratedFileData(
            "PLU FILE (MOCK)",
            "25H",
            37, //
            bytes
        );
    }
}