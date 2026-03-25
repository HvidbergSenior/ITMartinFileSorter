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
    }

    public bool NeedsConversion(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        // Skip MP4 completely
        return ext != ".mp4";
    }

    public async Task<string?> ConvertToMp4FastAsync(
        string inputPath,
        string outputFolder)
    {
        if (!NeedsConversion(inputPath))
            return null;

        Directory.CreateDirectory(outputFolder);

        var name = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(outputFolder, name + ".mp4");

        var arguments =
            $"-y -i \"{inputPath}\" " +
            "-c:v libx264 -preset veryfast -crf 23 " +
            "-c:a aac -b:a 128k " +
            "-movflags +faststart " +
            $"\"{outputPath}\"";

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

        // ⭐ Drain BOTH streams
        _ = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
                await process.StandardOutput.ReadLineAsync();
        });

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
                await process.StandardError.ReadLineAsync();
        });

        await process.WaitForExitAsync();

        return process.ExitCode == 0 ? outputPath : null;
    }
}