namespace ITMartinFileSorter.Application.Services;

public class FastVideoBatchExportService
{
    private readonly FastUniversalVideoConverterService _converter;

    public FastVideoBatchExportService(
        FastUniversalVideoConverterService converter)
    {
        _converter = converter;
    }

    public async Task ConvertAllVideosAsync(string exportRoot)
    {
        if (string.IsNullOrWhiteSpace(exportRoot) ||
            !Directory.Exists(exportRoot))
            return;

        var videoFiles = Directory
            .EnumerateFiles(exportRoot, "*.*", SearchOption.AllDirectories)
            .Where(IsVideoFile)
            .Where(_converter.NeedsConversion)
            .ToList();

        Console.WriteLine($"Found {videoFiles.Count} videos to convert");

        // ⭐ SEQUENTIAL (SAFE)
        foreach (var file in videoFiles)
        {
            var folder = Path.GetDirectoryName(file)!;

            Console.WriteLine($"Converting: {file}");

            await _converter.ConvertToMp4FastAsync(file, folder);
        }
    }

    private static bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext is
            ".avi" or ".mov" or ".mkv" or ".wmv" or
            ".flv" or ".m4v" or ".3gp" or ".mpg" or ".mpeg";
    }
}