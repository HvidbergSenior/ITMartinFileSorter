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
            "ffmpeg.exe"
        );

        Console.WriteLine($"FFmpeg path: {_ffmpegPath}");
        Console.WriteLine($"FFmpeg exists: {File.Exists(_ffmpegPath)}");
    }

    public bool NeedsConversion(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext != ".mp4";
    }

    public async Task<string?> ConvertToMp4FastAsync(
    string inputPath,
    string outputFolder)
{
    Console.WriteLine("===== DEBUG: CONVERTER =====");
    Console.WriteLine($"Input: {inputPath}");
    Console.WriteLine($"Extension: {Path.GetExtension(inputPath)}");
    Console.WriteLine($"NeedsConversion: {NeedsConversion(inputPath)}");
    Console.WriteLine($"FFmpeg exists: {File.Exists(_ffmpegPath)}");

    if (!NeedsConversion(inputPath))
    {
        Console.WriteLine("[CONVERT] Skipped (already MP4)");
        return null;
    }

    if (!File.Exists(_ffmpegPath))
        throw new FileNotFoundException("FFmpeg not found", _ffmpegPath);

    Directory.CreateDirectory(outputFolder);

    var name = Path.GetFileNameWithoutExtension(inputPath);
    var outputPath = Path.Combine(outputFolder, name + ".mp4");

    var arguments =
        $"-y -i \"{inputPath}\" " +
        "-vf \"scale=trunc(iw/2)*2:trunc(ih/2)*2,format=yuv420p\" " +
        "-pix_fmt yuv420p " +
        "-color_primaries bt709 -color_trc bt709 -colorspace bt709 " +
        "-c:v libx264 -preset veryfast -crf 23 " +
        "-c:a aac -b:a 128k " +
        "-movflags +faststart " +
        $"\"{outputPath}\"";

    Console.WriteLine($"[FFMPEG CMD] {_ffmpegPath} {arguments}");

    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }
    };

    process.Start();

    var stderrTask = process.StandardError.ReadToEndAsync();
    var stdoutTask = process.StandardOutput.ReadToEndAsync();

    await process.WaitForExitAsync();

    var stderr = await stderrTask;

    Console.WriteLine(stderr);
    Console.WriteLine($"[FFMPEG EXIT] {process.ExitCode}");

    if (process.ExitCode != 0)
    {
        if (File.Exists(outputPath))
            File.Delete(outputPath);

        throw new Exception("FFmpeg failed");
    }

    var created = File.GetCreationTime(inputPath);
    var modified = File.GetLastWriteTime(inputPath);

    if (created.Year < 2000)
        created = modified;

    File.SetCreationTime(outputPath, created);
    File.SetLastWriteTime(outputPath, modified);

// DELETE ORIGINAL AFTER SUCCESS
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

    return outputPath;
}
}