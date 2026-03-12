using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ITMartinFileScanner.Application.Helpers;

public static class GpsHelper
{
    public static (double lat, double lng)? GetCoordinates(string path)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(path);
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();

            if (gps == null)
                return null;

            var latValues = gps.GetRationalArray(GpsDirectory.TagLatitude);
            var lngValues = gps.GetRationalArray(GpsDirectory.TagLongitude);
            var latRef = gps.GetString(GpsDirectory.TagLatitudeRef);
            var lngRef = gps.GetString(GpsDirectory.TagLongitudeRef);

            if (latValues == null || lngValues == null ||
                latRef == null || lngRef == null)
                return null;

            double latitude = ToDegrees(latValues);
            double longitude = ToDegrees(lngValues);

            if (latRef != "N")
                latitude *= -1;

            if (lngRef != "E")
                longitude *= -1;

            return (latitude, longitude);
        }
        catch
        {
            return null;
        }
    }

    private static double ToDegrees(Rational[] values)
    {
        return values[0].ToDouble() +
               values[1].ToDouble() / 60.0 +
               values[2].ToDouble() / 3600.0;
    }
}