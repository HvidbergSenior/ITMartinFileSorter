namespace ITMartinFileSorter.Application.Services;

public class VideoExportService
{
    private readonly VideoConverterService _converter;

    public VideoExportService(VideoConverterService converter)
    {
        _converter = converter;
    }

    public async Task ExportVideosAsync(IEnumerable<string> sourceFiles, string sourceRoot)
    {
        var afterFolder = Path.Combine(sourceRoot, "After", "Videos");

        Directory.CreateDirectory(afterFolder);

        foreach (var file in sourceFiles)
        {
            try
            {
                await _converter.ConvertToMp4Async(file, afterFolder);
            }
            catch
            {
                // Ignore errors for now
            }
        }
    }
}