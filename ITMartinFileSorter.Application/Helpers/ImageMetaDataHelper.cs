using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ITMartinFileScanner.Application.Helpers;

public static class ImageMetadataHelper
{
    public static DateTime? GetCreationTime(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);

            var exif = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exif != null && exif.TryGetDateTime(ExifSubIfdDirectory.TagDateTimeOriginal, out var date))
            {
                return date;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}