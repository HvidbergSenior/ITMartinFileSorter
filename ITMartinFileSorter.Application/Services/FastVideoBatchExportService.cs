namespace ITMartinFileSorter.Application.Services;

public class FastVideoBatchExportService
{
    private readonly FastUniversalVideoConverterService _converter;

    public FastVideoBatchExportService(
        FastUniversalVideoConverterService converter)
    {
        _converter = converter;
    }

    public async Task ConvertAllVideosAsync(
        string exportRoot,
        Action<int, int, string>? progress = null)
    {
        Console.WriteLine("[BATCH] ConvertAllVideosAsync called");
        Console.WriteLine($"[BATCH] Export root: {exportRoot}");

        if (string.IsNullOrWhiteSpace(exportRoot) ||
            !Directory.Exists(exportRoot))
        {
            Console.WriteLine("[BATCH] Invalid export root");
            return;
        }

        var videoFiles = Directory
            .EnumerateFiles(exportRoot, "*.*", SearchOption.AllDirectories)
            .Where(IsVideoFile)
            .Where(_converter.NeedsConversion)
            .ToList();

        int total = videoFiles.Count;
        int current = 0;

        Console.WriteLine($"[BATCH] Videos found: {total}");

        foreach (var file in videoFiles)
        {
            current++;

            progress?.Invoke(
                current,
                total,
                Path.GetFileName(file));

            Console.WriteLine($"[BATCH] Converting {current}/{total}: {file}");

            var folder = Path.GetDirectoryName(file)!;

            await _converter.ConvertToMp4FastAsync(file, folder);

            Console.WriteLine($"[BATCH] Done file: {file}");
        }

        Console.WriteLine("[BATCH] All conversions done");
    }

    private static bool IsVideoFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext is
            ".avi" or ".mov" or ".mkv" or ".wmv" or
            ".flv" or ".m4v" or ".3gp" or ".mpg" or ".mpeg";
    }
}