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
        Console.WriteLine("===== DEBUG: BATCH START =====");
        Console.WriteLine($"Export root: {exportRoot}");

        if (string.IsNullOrWhiteSpace(exportRoot) ||
            !Directory.Exists(exportRoot))
        {
            Console.WriteLine("[BATCH] Invalid export root");
            return;
        }

        var allFiles = Directory
            .EnumerateFiles(exportRoot, "*.*", SearchOption.AllDirectories)
            .ToList();

        Console.WriteLine($"Total files found: {allFiles.Count}");

        foreach (var f in allFiles.Take(20))
        {
            Console.WriteLine($"[BATCH FILE] {f}");
        }

        var videoFiles = allFiles
            .Where(IsVideoFile)
            .Where(_converter.NeedsConversion)
            .ToList();

        Console.WriteLine($"[BATCH] Videos after filter: {videoFiles.Count}");

        foreach (var v in videoFiles.Take(20))
        {
            Console.WriteLine($"[BATCH VIDEO] {v}");
        }

        int total = videoFiles.Count;
        int current = 0;

        foreach (var file in videoFiles)
        {
            Console.WriteLine($"[BATCH] Converting: {file}");

            var folder = Path.GetDirectoryName(file)!;

            try
            {
                var output = await _converter.ConvertToMp4FastAsync(file, folder);

                Console.WriteLine($"[BATCH] Done file: {file}");

                if (output != null && File.Exists(output))
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {file}: {ex}");
                throw; // 👈 IMPORTANT
            }

            var done = Interlocked.Increment(ref current);

            progress?.Invoke(
                done,
                total,
                Path.GetFileName(file));
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