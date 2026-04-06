using System.Diagnostics;

namespace ITMartinFileSorter.Application.Services;

public class FastUniversalVideoConverterService
{
    private readonly string _ffmpegPath;

    public FastUniversalVideoConverterService()
    {
        _ffmpegPath = Path.Combine(
            AppContext.BaseDirectory,
            "ffmpeg",
            "ffmpeg.exe");

        Console.WriteLine($"FFmpeg path: {_ffmpegPath}");
        Console.WriteLine($"FFmpeg exists: {File.Exists(_ffmpegPath)}");
    }

    public bool NeedsConversion(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext is
            ".avi" or ".mov" or ".mkv" or ".wmv" or
            ".flv" or ".m4v" or ".3gp" or ".mpg" or ".mpeg";
    }

    public async Task<string?> ConvertToMp4FastAsync(
        string inputPath,
        string outputFolder)
    {
        Console.WriteLine("===== DEBUG: CONVERTER =====");
        Console.WriteLine($"Input: {inputPath}");

        if (!NeedsConversion(inputPath))
        {
            Console.WriteLine("[CONVERT] Skipped");
            return null;
        }

        if (!File.Exists(_ffmpegPath))
            throw new FileNotFoundException("FFmpeg not found", _ffmpegPath);

        Directory.CreateDirectory(outputFolder);

        var name = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(outputFolder, name + ".mp4");

        try
        {
            // FAST PATH: stream copy (very fast)
            Console.WriteLine("[FFMPEG] Trying stream copy...");

            await RunFfmpegAsync(
                $"-y -i \"{inputPath}\" " +
                "-c copy " +
                "-movflags +faststart " +
                $"\"{outputPath}\"");

            await WaitForOutputReady(outputPath);

            CopyDates(inputPath, outputPath);
            Console.WriteLine("[FFMPEG] Stream copy success");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FFMPEG] Stream copy failed: {ex.Message}");
            Console.WriteLine("[FFMPEG] Falling back to re-encode...");

            // FALLBACK: full conversion only if needed
            await RunFfmpegAsync(
                $"-y -i \"{inputPath}\" " +
                "-c:v libx264 " +
                "-preset veryfast " +
                "-crf 23 " +
                "-c:a aac " +
                "-b:a 128k " +
                "-movflags +faststart " +
                $"\"{outputPath}\"");

            Console.WriteLine("[FFMPEG] Re-encode success");
        }

        CopyDates(inputPath, outputPath);

        TryDeleteOriginal(inputPath, outputPath);

        return outputPath;
    }

    private async Task RunFfmpegAsync(string arguments)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        var errorTask = process.StandardError.ReadToEndAsync();
        var outputTask = process.StandardOutput.ReadToEndAsync();

        await process.WaitForExitAsync();

        var error = await errorTask;
        var output = await outputTask;

        Console.WriteLine(output);
        Console.WriteLine(error);
        Console.WriteLine($"[FFMPEG EXIT] {process.ExitCode}");

        if (process.ExitCode != 0)
            throw new Exception("FFmpeg failed");
    }

    private void CopyDates(string inputPath, string outputPath)
    {
        var created = File.GetCreationTime(inputPath);
        var modified = File.GetLastWriteTime(inputPath);

        if (created.Year < 2000)
            created = modified;

        File.SetCreationTime(outputPath, created);
        File.SetLastWriteTime(outputPath, modified);
    }

    private void TryDeleteOriginal(string inputPath, string outputPath)
    {
        if (File.Exists(outputPath) &&
            !inputPath.Equals(outputPath, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                File.Delete(inputPath);
                Console.WriteLine($"[ORIGINAL DELETED] {inputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DELETE FAILED] {ex}");
            }
        }
    }
    
    private async Task WaitForOutputReady(string outputPath)
    {
        for (int i = 0; i < 20; i++)
        {
            try
            {
                using var stream = File.Open(
                    outputPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

                if (stream.Length > 0)
                    return;
            }
            catch
            {
                // still locked
            }

            await Task.Delay(250);
        }
    }
}