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

            var name = Path.GetFileName(file).ToLowerInvariant();
            var folder = Path.GetDirectoryName(file)?.ToLowerInvariant() ?? "";

            bool isDownload = folder.Contains("download");
            bool isWhatsapp = folder.Contains("whatsapp");
            bool isTelegram = folder.Contains("telegram");

            // ---------- IMAGE ----------
            if (type == MediaType.Image)
            {
                if (name.Contains("screenshot"))
                    mediaFile.SubCategory = MediaSubCategory.Screenshot;

                else if (extension is ".heic" or ".heif")
                    mediaFile.SubCategory = MediaSubCategory.PhonePhoto;

                else if (name.StartsWith("img_") || name.StartsWith("dsc_"))
                    mediaFile.SubCategory = MediaSubCategory.Camera;

                else if (isWhatsapp)
                    mediaFile.SubCategory = MediaSubCategory.WhatsApp;

                else if (isDownload)
                    mediaFile.SubCategory = MediaSubCategory.Download;

                else
                    mediaFile.SubCategory = MediaSubCategory.OtherImage;
            }

            // ---------- VIDEO ----------
            else if (type == MediaType.Video)
            {
                if (name.Contains("screen"))
                    mediaFile.SubCategory = MediaSubCategory.ScreenRecording;

                else if (name.StartsWith("img_"))
                    mediaFile.SubCategory = MediaSubCategory.PhoneVideo;

                else if (isWhatsapp)
                    mediaFile.SubCategory = MediaSubCategory.WhatsApp;

                else if (isDownload)
                    mediaFile.SubCategory = MediaSubCategory.Download;

                else
                    mediaFile.SubCategory = MediaSubCategory.OtherVideo;
            }

            // ---------- AUDIO ----------
            else if (type == MediaType.Audio)
            {
                if (extension is ".mp3" or ".flac")
                    mediaFile.SubCategory = MediaSubCategory.Music;

                else if (extension is ".wav" or ".aac")
                    mediaFile.SubCategory = MediaSubCategory.VoiceMemo;

                else
                    mediaFile.SubCategory = MediaSubCategory.UnknownAudio;
            }

            // ---------- DOCUMENT ----------
            else if (type == MediaType.Document)
            {
                mediaFile.SubCategory = extension switch
                {
                    ".pdf" => MediaSubCategory.Pdf,
                    ".doc" or ".docx" => MediaSubCategory.Word,
                    ".xls" or ".xlsx" => MediaSubCategory.Excel,
                    ".txt" => MediaSubCategory.Text,
                    ".ppt" or ".pptx" => MediaSubCategory.Presentation,
                    ".csv" => MediaSubCategory.Csv,
                    _ => MediaSubCategory.UnknownDocument
                };
            }

            yield return mediaFile;
        }
    }
}