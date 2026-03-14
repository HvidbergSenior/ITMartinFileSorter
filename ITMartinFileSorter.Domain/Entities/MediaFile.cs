using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Domain.Entities;

public class MediaFile
{
    public string FullPath { get; }
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public MediaType Type { get; }

    // Categorization
    public MediaMainCategory MainCategory { get; set; }
    public MediaSubCategory SubCategory { get; set; }
    public MediaTertiaryCategory TertiaryCategory { get; set; }

    // Metadata
    public string? DeviceModel { get; set; }
    public string? Location { get; set; }
    public string? Hash { get; private set; }

    public string? DynamicFolder { get; set; }

    // --- Video-specific metadata ---
    public long? DurationMs { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }

    public MediaFile(string fullPath, long size, DateTime createdAt, MediaType type, long sizeBytes)
    {
        FullPath = fullPath;
        SizeBytes = sizeBytes;
        CreatedAt = createdAt;
        Type = type;

        MainCategory = type switch
        {
            MediaType.Audio => MediaMainCategory.Audio,
            MediaType.Video => MediaMainCategory.Video,
            MediaType.Document => MediaMainCategory.Document,
            MediaType.Image => MediaMainCategory.Image,
            _ => MediaMainCategory.Image
        };

        SubCategory = type switch
        {
            MediaType.Audio => MediaSubCategory.UnknownAudio,
            MediaType.Video => MediaSubCategory.UnknownVideo,
            MediaType.Document => MediaSubCategory.UnknownDocument,
            MediaType.Image => MediaSubCategory.UnknownImage,
            _ => MediaSubCategory.UnknownImage
        };

        TertiaryCategory = MediaTertiaryCategory.Unknown;
    }

    public void SetHash(string hash) => Hash = hash;

    public void SetVideoMetadata(long? durationMs, int? width, int? height)
    {
        DurationMs = durationMs;
        Width = width;
        Height = height;
    }
}
