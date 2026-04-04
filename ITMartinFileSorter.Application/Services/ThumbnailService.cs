using System.Diagnostics;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ITMartinFileSorter.Application.Services;

public class ThumbnailService
{
    private readonly string _thumbnailRoot;
    private readonly string _webThumbnailPath = "/media_temp/thumbnails";

    public ThumbnailService()
    {
        _thumbnailRoot = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "media_temp",
            "thumbnails");

        Directory.CreateDirectory(_thumbnailRoot);
    }

    public string? GenerateThumbnail(MediaFile file)
    {
        try
        {
            var extension = ".jpg";
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullOutputPath = Path.Combine(_thumbnailRoot, fileName);

            switch (file.Type)
            {
                case MediaType.Image:
                    GenerateImageThumbnail(file.FullPath, fullOutputPath);
                    break;

                case MediaType.Video:
                    GenerateVideoThumbnail(file.FullPath, fullOutputPath);
                    break;

                default:
                    return null;
            }

            if (!File.Exists(fullOutputPath))
                return null;

            return $"{_webThumbnailPath}/{fileName}";
        }
        catch
        {
            return null;
        }
    }

    private void GenerateImageThumbnail(string inputPath, string outputPath)
    {
        using var image = Image.Load(inputPath);

        image.Mutate(x =>
            x.Resize(new ResizeOptions
            {
                Size = new Size(180, 180),
                Mode = ResizeMode.Max
            }));

        image.SaveAsJpeg(outputPath);
    }

    private void GenerateVideoThumbnail(string inputPath, string outputPath)
    {
        var ffmpegPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "ffmpeg",
            "ffmpeg.exe");

        if (!File.Exists(ffmpegPath))
            return;

        using var process = new Process();

        process.StartInfo.FileName = ffmpegPath;
        process.StartInfo.Arguments =
            $"-y -i \"{inputPath}\" -ss 00:00:01 -vframes 1 -vf scale=180:-1 \"{outputPath}\"";
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        process.Start();
        process.WaitForExit();
    }
}