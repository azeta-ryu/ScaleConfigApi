using System.Diagnostics;
using System.Net;
using System.Text;
using ScaleConfigApi.Models;

namespace ScaleConfigApi.Services;

/// <summary>
/// Handles saving generated files and executing the DIGIWTCP command-line tool.
/// </summary>
public class ScaleUploaderService(ILogger<ScaleUploaderService> logger)
{
    private readonly ILogger<ScaleUploaderService> _logger = logger;

    // Assumes TWSWTCP.exe is in the same folder as the API.
    private readonly string _driverPath = Path.Combine(
        AppContext.BaseDirectory, "TWSWTCP.exe");

    public async Task<ScaleUploadResult> UploadFilesAsync(
        string scaleIpAddress,
        List<GeneratedFileData> files)
    {
        var uploadLog = new List<string>();

        // 1. Validate the IP Address to prevent command injection
        if (!IPAddress.TryParse(scaleIpAddress, out _))
        {
            Log.UploadFailedInvalidIp(_logger, scaleIpAddress);
            return new ScaleUploadResult(
                "Upload failed: Invalid IP address.",
                [$"Error: '{scaleIpAddress}' is not a valid IP."]
            );
        }

        if (!File.Exists(_driverPath))
        {
            Log.UploadFailedNoDriver(_logger, _driverPath);
            return new ScaleUploadResult(
                "Upload failed: Driver not found.",
                [$"Error: '{_driverPath}' not found."]
            );
        }

        // 2. The driver requires files to be in the current directory
        // We will temporarily write them, execute the process, then delete them.
        var filePaths = new List<string>();

        try
        {
            // 3. Write files to disk
            foreach (var file in files)
            {
                // Format: SM[Scale_IP]F[File_Decimal].DAT
                string fileName = $"SM{scaleIpAddress}F{file.FileNumberDecimal}.DAT";

                // Use the app's base directory as the working directory
                string filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                await File.WriteAllBytesAsync(filePath, file.Content);
                filePaths.Add(filePath);
                Log.FileWritten(_logger, filePath);
                uploadLog.Add($"Successfully wrote file: {fileName}");
            }

            // 4. Execute upload commands in order
            // The 25H file must be sent last to trigger the update
            foreach (var file in files.OrderBy(f => f.FileNumberHex == "25H" ? 1 : 0))
            {
                // Command: TWSWTCP WR [File_Number] [Scale_IP]
                string arguments = $"WR {file.FileNumberDecimal} {scaleIpAddress}";
                uploadLog.Add($"Executing: TWSWTCP.exe {arguments}");
                Log.ExecutingProcess(_logger, _driverPath, arguments);

                var (exitCode, output) = await RunProcessAsync(arguments);

                if (exitCode == 0)
                {
                    uploadLog.Add($"Success (Exit Code {exitCode}).");
                    uploadLog.AddRange(output);
                }
                else
                {
                    Log.ProcessError(_logger, exitCode, string.Join("\n", output));
                    uploadLog.Add($"Error (Exit Code {exitCode}).");
                    uploadLog.AddRange(output);

                    // Stop on first error
                    return new ScaleUploadResult("Upload failed.", uploadLog);
                }
            }
        }
        catch (Exception ex)
        {
            Log.UploadFailed(_logger, ex.Message, ex);
            uploadLog.Add($"An unhandled exception occurred: {ex.Message}");
            return new ScaleUploadResult("Upload failed with an exception.", uploadLog);
        }
        finally
        {
            // 5. Clean up temporary .DAT files
            foreach (var path in filePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                        uploadLog.Add($"Cleaned up: {Path.GetFileName(path)}");
                    }
                }
                catch (Exception ex)
                {
                    Log.FileCleanupFailed(_logger, path, ex.Message, ex);
                    uploadLog.Add($"Warning: Failed to clean up {path}.");
                }
            }
        }

        return new ScaleUploadResult("Upload commands executed successfully.", uploadLog);
    }

    /// <summary>
    /// Executes the TWSWTCP.exe process asynchronously.
    /// </summary>
    private async Task<(int ExitCode, List<string> Output)> RunProcessAsync(string arguments)
    {
        var outputLog = new List<string>();

        var processStartInfo = new ProcessStartInfo
        {
            FileName = _driverPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        };

        using var process = new Process { StartInfo = processStartInfo };

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null) outputLog.Add($"[OUT] {args.Data}");
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null) outputLog.Add($"[ERR] {args.Data}");
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return (process.ExitCode, outputLog);
    }
}