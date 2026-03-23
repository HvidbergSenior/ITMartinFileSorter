using ITMartinFileSorter.Application.Services;

namespace ITMartinFileSorter.Application.UseCases;

public class FastVideoBatchExportService
{
    private readonly FastUniversalVideoConverterService _converter;

    public FastVideoBatchExportService(FastUniversalVideoConverterService converter)
    {
        _converter = converter;
    }

    public async Task ConvertAllVideosAsync(IEnumerable<string> files, string sourceRoot)
    {
        var outputFolder = Path.Combine(sourceRoot, "After", "Videos");

        var tasks = files
            .Where(_converter.NeedsConversion)
            .Select(file => _converter.ConvertToMp4FastAsync(file, outputFolder));

        await Task.WhenAll(tasks);
    }
}