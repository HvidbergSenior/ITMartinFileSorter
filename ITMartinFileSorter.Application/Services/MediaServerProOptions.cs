namespace ITMartinFileSorter.Application.Services;

public class MediaServerProOptions
{
    public bool EnableAutoOptimizeAfterExport { get; set; } = false;

    public bool DeleteOriginalVideos { get; set; } = false;

    public long MinVideoSizeBytes { get; set; } = 20_000_000; // 20 MB

    public int MaxParallelJobs { get; set; } =
        Math.Max(1, Environment.ProcessorCount / 2);
}