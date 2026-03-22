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

    public IEnumerable<MediaFile> ScanFolder(string rootPath, Action<int, string>? onProgress = null)
    {
        int index = 0;

        foreach (var file in Directory.EnumerateFiles(rootPath, "*.*", _options))
        {
            index++;
            onProgress?.Invoke(index, file);

            var extension = Path.GetExtension(file);

            var isImage = ImageExtensions.Contains(extension);
            var isVideo = VideoExtensions.Contains(extension);
            var isAudio = AudioExtensions.Contains(extension);
            var isDocument = DocumentExtensions.Contains(extension);

            if (!isImage && !isVideo && !isAudio && !isDocument)
                continue;

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

            yield return mediaFile;
        }
    }
}