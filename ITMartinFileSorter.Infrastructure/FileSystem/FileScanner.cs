using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;
using ITMartinFileSorter.Domain.Interfaces;

namespace ITMartinFileSorter.Infrastructure.FileSystem;

public sealed class FileScanner : IFileScanner
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".tif", ".heic", ".heif", ".avif"
    };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mov", ".avi", ".mkv", ".wmv"
    };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp3", ".wav", ".flac", ".aac", ".ogg"
    };

    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx", ".ppt", ".pptx", ".csv"
    };

    private readonly EnumerationOptions _options = new()
    {
        RecurseSubdirectories = true,
        IgnoreInaccessible = true,
        ReturnSpecialDirectories = false
    };

    public IEnumerable<MediaFile> ScanFolder(string rootPath)
{
    foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", _options))
    {
        var extension = Path.GetExtension(file).ToLowerInvariant();

        var isImage = ImageExtensions.Contains(extension);
        var isVideo = VideoExtensions.Contains(extension);
        var isAudio = AudioExtensions.Contains(extension);
        var isDocument = DocumentExtensions.Contains(extension);

        if (!isImage && !isVideo && !isAudio && !isDocument) continue;

        FileInfo info;
        try { info = new FileInfo(file); }
        catch { continue; }

        var type = isImage ? MediaType.Image
                  : isVideo ? MediaType.Video
                  : isAudio ? MediaType.Audio
                  : MediaType.Document;

        var mediaFile = new MediaFile(
            fullPath: file,
            createdAt: info.LastWriteTimeUtc,
            type: type,
            sizeBytes: info.Length
        );

        // --- Assign proper subcategory ---
        if (type == MediaType.Image)
        {
            mediaFile.SubCategory = extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp" or ".tiff" or ".tif" => MediaSubCategory.Screenshot,
                ".heic" or ".heif" => MediaSubCategory.PhoneScreenshot,
                _ => MediaSubCategory.UnknownImage
            };
        }
        else if (type == MediaType.Video)
        {
            mediaFile.SubCategory = extension switch
            {
                ".mp4" or ".mov" => MediaSubCategory.Movie,
                ".avi" or ".mkv" or ".wmv" => MediaSubCategory.Clip,
                _ => MediaSubCategory.UnknownVideo
            };
        }
        else if (type == MediaType.Audio)
        {
            mediaFile.SubCategory = extension switch
            {
                ".mp3" or ".flac" => MediaSubCategory.Music,
                ".wav" or ".aac" => MediaSubCategory.VoiceMemo,
                _ => MediaSubCategory.UnknownAudio
            };
        }
        else if (type == MediaType.Document)
        {
            mediaFile.SubCategory = extension switch
            {
                ".pdf" => MediaSubCategory.Pdf,
                ".doc" or ".docx" => MediaSubCategory.Word,
                ".xls" or ".xlsx" => MediaSubCategory.Excel,
                ".txt" => MediaSubCategory.Text,
                ".ppt" or ".pptx" => MediaSubCategory.Presentation,
                _ => MediaSubCategory.UnknownDocument
            };
        }

        yield return mediaFile;
    }
}
}