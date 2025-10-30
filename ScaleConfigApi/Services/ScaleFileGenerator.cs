using System.Text;
using ScaleConfigApi.Models;

namespace ScaleConfigApi.Services;

/// <summary>
/// Generates the binary configuration files for the DIGI scale.
/// </summary>
public class ScaleFileGenerator(ILogger<ScaleFileGenerator> logger)
{
    private readonly ILogger<ScaleFileGenerator> _logger = logger;

    public ScaleFileGenerationResult GenerateFiles(ProductTagLink[] products)
    {
        var files = new List<ScaleFile>
        {
            GenerateAahFile(products),
            GenerateCchFile(products),
            Generate25hFileMock() // The spec for 25H is not provided
        };

        return new ScaleFileGenerationResult(Files: files);
    }

    /// <summary>
    /// Generates the AA PLU4 FILE (AAH)
    /// This file links supplementary e-Label data (like images) to the PLUs.
    /// </summary>
    private ScaleFile GenerateAahFile(ProductTagLink[] products)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            foreach (var product in products)
            {
                // Calculate record size: Based on spec (4+4+2+4+4+4+4+4 = 30 bytes)
                // We assume 0-length ELABEL COMMODITY for this example.
                short recordSize = 30; 

                // 1. PLU4 NUMBER (4 Bytes, BCD)
                BinaryWriterHelper.WriteBcd(writer, product.PluNumber, 4);

                // 2. PLU4 RECORD SIZE (2 Bytes, HEX)
                writer.Write(recordSize);

                // 3. STATUS (4 Bytes, BIN)
                // Bit 5 must be set to 1 to "Update E-label"
                //
                int status = 1 << 5; 
                writer.Write(status);

                // 4. E - LABEL ADDRESS (4 Bytes, HEX) - Default to 0
                writer.Write(0);

                // 5. E - LABEL FORMAT (2 Bytes, BCD) - Default to 0
                BinaryWriterHelper.WriteBcd(writer, 0, 2);

                // 6. ELABEL IMAGE 1 (4 Bytes, BCD)
                BinaryWriterHelper.WriteBcd(writer, product.ImageId, 4);

                // 7. ELABEL IMAGE 2 (4 Bytes, BCD) - Default to 0
                BinaryWriterHelper.WriteBcd(writer, 0, 4);

                // 8. ELABEL IMAGE 3 (4 Bytes, BCD) - Default to 0
                BinaryWriterHelper.WriteBcd(writer, 0, 4);

                // 9. ELABEL MIN TEMPERATURE (4 Bytes, BCD) - Default to 0
                BinaryWriterHelper.WriteBcd(writer, 0, 4);

                // 10. ELABEL MAX TEMPERATURE (4 Bytes, BCD) - Default to 0
                BinaryWriterHelper.WriteBcd(writer, 0, 4);

                // 11. ELABEL COMMODITY (ASCII) - 0 bytes for this example
            }
        }
        
        var bytes = ms.ToArray();
        Log.FileGenerationComplete(_logger, "AA PLU4 FILE", "AAH", bytes.Length);
        
        return new ScaleFile(
            "AA PLU4 FILE",
            "AAH",
            170, //
            Convert.ToBase64String(bytes)
        );
    }

    /// <summary>
    /// Generates the CC TAG INFO FILE (CCH)
    /// This file creates the link between a PLU and a physical 9-digit Tag ID.
    /// </summary>
    private ScaleFile GenerateCchFile(ProductTagLink[] products)
    {
        using var ms = new MemoryStream();
        using (var writer = new BinaryWriter(ms, Encoding.ASCII, leaveOpen: true))
        {
            foreach (var product in products)
            {
                // Calculate record size: 100 (NAME) + 19 * 4-byte fields = 176
                short recordSize = 176;

                // 1. TAGID (4 Bytes, INTEGER)
                // The 9-digit Tag ID is stored as a standard 4-byte int
                writer.Write(int.Parse(product.TagId));

                // 2. RECORD SIZE (2 Bytes, HEX)
                writer.Write(recordSize);

                // 3. TAG NAME (100 Bytes, CHAR)
                BinaryWriterHelper.WritePaddedString(writer, product.ProductName, 100);

                // 4. TAG TYPE (4 Bytes, INTEGER)
                // 999 = Auto init
                writer.Write(999); 

                // 5. PLU NUMBER (4 Bytes, INTEGER)
                writer.Write(product.PluNumber);

                // Write 0 for all other fields (17 fields * 4 bytes)
                // TRACEABILITY_NUMBER -> SENSITIVITY_UPPER_LIMIT
                for (int i = 0; i < 17; i++)
                {
                    writer.Write(0);
                }
            }
        }
        
        var bytes = ms.ToArray();
        Log.FileGenerationComplete(_logger, "CC TAG INFO FILE", "CCH", bytes.Length);

        return new ScaleFile(
            "CC TAG INFO FILE",
            "CCH",
            204, //
            Convert.ToBase64String(bytes)
        );
    }

    /// <summary>
    /// Generates a MOCK PLU FILE (25H).
    /// The documentation states this file is required but does not provide
    /// its structure.
    /// </summary>
    private ScaleFile Generate25hFileMock()
    {
        Log.MockFileGenerated(_logger, "PLU FILE (25H)");
        string mockContent = 
            "// MOCK FILE - The structure for 25H (PLU FILE) was not " + 
            "// provided in the setup documentation. " +
            "// This file is required to trigger the tag update.";
            
        var bytes = Encoding.ASCII.GetBytes(mockContent);
        
        return new ScaleFile(
            "PLU FILE (MOCK)",
            "25H",
            37, //
            Convert.ToBase64String(bytes)
        );
    }
}