using System.IO;

namespace ITMartinFileSorter.Application.Services;

public class MediaServerProService
{
    private readonly VideoConverterService _converter;
    private readonly MediaServerProOptions _options;

    public MediaServerProService(
        VideoConverterService converter,
        MediaServerProOptions options)
    {
        _converter = converter;
        _options = options;
    }

    public async Task OptimizeExportAsync(
        string exportRoot,
        Action<string>? progress = null)
    {
        var videosFolder = Path.Combine(exportRoot, "Videos");

        if (!Directory.Exists(videosFolder))
        {
            progress?.Invoke("No Videos folder found.");
            return;
        }

        progress?.Invoke("Scanning videos...");

        // 👉 IMPORTANT: forward progress directly
        await _converter.ConvertFolderAsync(
            videosFolder,
            progress,
            _options.DeleteOriginalVideos);
    }
}