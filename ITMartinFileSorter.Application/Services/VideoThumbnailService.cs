using System.Diagnostics;

namespace ITMartinFileSorter.Application.Services;

public class VideoThumbnailService
{
    private readonly string _tempFolder;

    public VideoThumbnailService(string tempFolder = null!)
    {
        _tempFolder = tempFolder ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media_temp");
        if (!Directory.Exists(_tempFolder))
            Directory.CreateDirectory(_tempFolder);
    }

    public string GenerateThumbnail(string videoPath)
    {
        if (!File.Exists(videoPath))
            throw new FileNotFoundException("Video file not found", videoPath);

        var safeName = Path.GetFileNameWithoutExtension(videoPath)
            .Replace(" ", "_")
            .Replace(".", "_");

        var thumbFileName = safeName + "_thumb.jpg";
        var thumbFullPath = Path.Combine(_tempFolder, thumbFileName);

        if (!File.Exists(thumbFullPath))
        {
            RunFFmpeg(videoPath, thumbFullPath);
        }

        return thumbFileName;
    }

    private void RunFFmpeg(string videoPath, string outputPath)
    {
        var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "Ffmeg", "bin", "ffmpeg.exe");

        var args = $"-y -i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{outputPath}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (!File.Exists(outputPath))
            throw new Exception($"FFmpeg failed to generate thumbnail for {videoPath}");
    }
}