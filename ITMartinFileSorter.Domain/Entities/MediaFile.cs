using ITMartinFileScanner.Domain.Enums;

namespace ITMartinFileScanner.Domain.Entities;

public class MediaFile
{
    public string FullPath { get; }
    public long Size { get; }
    public DateTime CreatedAt { get; }
    public MediaType Type { get; }
    public string? DynamicFolder { get; set; }
    public long SizeBytes { get; set; }
    public string? Hash { get; private set; }
    public MediaMainCategory MainCategory { get; set; }
    public MediaSubCategory SubCategory { get; set; }
    public long? DurationMs { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }

    public MediaFile(string fullPath, long size, DateTime createdAt, MediaType type, long sizeBytes, string? dynamicFolder)
    {
        FullPath = fullPath;
        Size = size;
        CreatedAt = createdAt;
        Type = type;
        SizeBytes = sizeBytes;
        DynamicFolder = dynamicFolder;

        MainCategory = MediaMainCategory.Review;
        SubCategory = MediaSubCategory.Unknown;
    }

    public void SetHash(string hash) => Hash = hash;
    
    public void SetVideoMetadata(long? durationMs, int? width, int? height)
    {
        DurationMs = durationMs;
        Width = width;
        Height = height;
    }
}