namespace ITMartinFileSorter.Application.Services;

public class FastVideoBatchExportService
{
    private readonly FastUniversalVideoConverterService _converter;

    public FastVideoBatchExportService(FastUniversalVideoConverterService converter)
    {
        _converter = converter;
    }

    /// <summary>
    /// Converts all videos inside the exported "After" folder (recursively)
    /// </summary>
    public async Task ConvertAllVideosAsync(string exportRoot)
    {
        if (string.IsNullOrWhiteSpace(exportRoot) || !Directory.Exists(exportRoot))
            return;

        var videoFiles = Directory
            .EnumerateFiles(exportRoot, "*.*", SearchOption.AllDirectories)
            .Where(IsVideoFile)
            .ToList();

        var tasks = videoFiles
            .Where(_converter.NeedsConversion)
            .Select(file =>
            {
                var outputFolder = Path.GetDirectoryName(file)!;

                return _converter.ConvertToMp4FastAsync(file, outputFolder);
            });

        await Task.WhenAll(tasks);
    }

    private static bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext is
            ".avi" or ".mov" or ".mkv" or ".wmv" or
            ".flv" or ".m4v" or ".3gp" or ".mpg" or ".mpeg";
    }
}