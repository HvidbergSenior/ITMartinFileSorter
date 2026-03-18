namespace ITMartinFileSorter.Application.Services;

public class VideoThumbnailService
{
    private readonly string _outputFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media_temp");

    public VideoThumbnailService()
    {
        if (!Directory.Exists(_outputFolder))
            Directory.CreateDirectory(_outputFolder);
    }

    public string GenerateThumbnail(string videoPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(videoPath) + ".png";
        string outputPath = Path.Combine(_outputFolder, fileName);

        if (File.Exists(outputPath)) return fileName; // return filename, not full path

        var ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");

        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = ffmpegPath,
            Arguments = $"-y -i \"{videoPath}\" -ss 00:00:01 -vframes 1 \"{outputPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        process.WaitForExit();

        return fileName;
    }
}