using System.Diagnostics;

namespace ITMartinFileSorter.Application.Services;

public class VideoConverterService
{
    private readonly string _ffmpegPath;

    public VideoConverterService()
    {
        Path.Combine(AppContext.BaseDirectory, "ffmpeg", "ffmpeg.exe");

        if (!File.Exists(_ffmpegPath))
            throw new FileNotFoundException($"ffmpeg.exe not found at {_ffmpegPath}");
    }

    public bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext is ".mp4" or ".mkv" or ".avi" or ".mov" or ".wmv" or ".flv" or ".webm";
    }

    public async Task<string?> ConvertToMp4Async(string inputPath, string outputFolder)
    {
        if (!IsVideoFile(inputPath))
            return null;

        var fileName = Path.GetFileNameWithoutExtension(inputPath);
        var outputPath = Path.Combine(outputFolder, fileName + ".mp4");

        // Skip if already MP4
        if (Path.GetExtension(inputPath).Equals(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(inputPath, outputPath, true);
            return outputPath;
        }

        var arguments =
            $"-y -i \"{inputPath}\" -c:v libx264 -preset medium -crf 23 -c:a aac -b:a 128k \"{outputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode == 0 ? outputPath : null;
    }
}