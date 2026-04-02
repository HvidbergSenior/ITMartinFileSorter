using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public static class AlbumStyleNameBuilder
{
    public static string Build(MediaFile file, int index)
    {
        var path = file.FullPath;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        // ⭐ BEST DATE AVAILABLE
        var date =
            ImageMetadataHelper.GetCreationTime(path) ??
            VideoMetadataHelper.GetCreationTime(path) ??
            file.CreatedAt;

        string monthYear = date.ToString("MMMMyyyy");

        // ⭐ LOCATION
        string location = GetLocation(file);

        // ⭐ TYPE
        string type = GetTypeLabel(file.SubCategory, file.MainCategory);

        // ⭐ BUILD NAME
        var parts = new List<string>
        {
            monthYear
        };

        if (!string.IsNullOrWhiteSpace(location))
            parts.Add(location);

        if (!string.IsNullOrWhiteSpace(type))
            parts.Add(type);

        parts.Add(index.ToString("D3"));

        return string.Join("-", parts) + ext;
    }

    private static string GetLocation(MediaFile file)
    {
        var coords = GpsHelper.GetCoordinates(file.FullPath);

        if (coords == null)
            return "";

        return LocationFilter.GetLocationName(
            coords.Value.lat,
            coords.Value.lng);
    }

    private static string GetTypeLabel(MediaSubCategory sub)
    {
        return sub switch
        {
            MediaSubCategory.Screenshot => "Screenshot",
            MediaSubCategory.ScreenRecording => "ScreenRecording",
            MediaSubCategory.PhonePhoto => "Photo",
            MediaSubCategory.Camera => "Photo",
            MediaSubCategory.OtherImage => "Image",

            MediaSubCategory.PhoneVideo => "Video",
            MediaSubCategory.Movie => "Movie",
            MediaSubCategory.Clip => "Clip",
            MediaSubCategory.OtherVideo => "Video",

            MediaSubCategory.Pdf => "PDF",
            MediaSubCategory.Word => "Document",
            MediaSubCategory.Excel => "Spreadsheet",
            MediaSubCategory.Presentation => "Presentation",
            MediaSubCategory.Text => "Text",

            MediaSubCategory.Music => "Music",
            MediaSubCategory.VoiceMemo => "VoiceMemo",

            _ => "File"
        };
    }

    /// <summary>
    /// Ensures the filename is unique inside a folder.
    /// </summary>
    public static string EnsureUnique(string folder, string fileName)
    {
        string fullPath = Path.Combine(folder, fileName);

        if (!File.Exists(fullPath))
            return fileName;

        string name = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);

        int i = 1;

        while (true)
        {
            string newName = $"{name}_{i}{ext}";
            string newPath = Path.Combine(folder, newName);

            if (!File.Exists(newPath))
                return newName;

            i++;
        }
    }
    private static string GetTypeLabel(MediaSubCategory sub, MediaMainCategory main)
    {
        // ⭐ Prefer specific types first
        switch (sub)
        {
            // Images
            case MediaSubCategory.Screenshot:
                return "Screenshot";

            case MediaSubCategory.ScreenRecording:
                return "ScreenRecording";

            case MediaSubCategory.PhonePhoto:
            case MediaSubCategory.Camera:
            case MediaSubCategory.OtherImage:
            case MediaSubCategory.UnknownImage:
                return "Photo";

            // Videos
            case MediaSubCategory.PhoneVideo:
            case MediaSubCategory.Movie:
            case MediaSubCategory.Clip:
            case MediaSubCategory.OtherVideo:
            case MediaSubCategory.UnknownVideo:
                return "Video";

            // Documents
            case MediaSubCategory.Pdf:
                return "PDF";

            case MediaSubCategory.Word:
            case MediaSubCategory.Excel:
            case MediaSubCategory.Presentation:
            case MediaSubCategory.Text:
            case MediaSubCategory.Csv:
            case MediaSubCategory.UnknownDocument:
                return "Document";

            // Audio
            case MediaSubCategory.Music:
            case MediaSubCategory.VoiceMemo:
            case MediaSubCategory.UnknownAudio:
                return "Audio";
        }

        // ⭐ FINAL fallback: main category
        return main switch
        {
            MediaMainCategory.Image => "Photo",
            MediaMainCategory.Video => "Video",
            MediaMainCategory.Document => "Document",
            MediaMainCategory.Audio => "Audio",
            _ => "File"
        };
    }
}