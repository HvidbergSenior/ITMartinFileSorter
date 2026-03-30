using System.Text.RegularExpressions;
using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public class TripGroupingService
{
    public List<TripGroup> CreateTrips(IEnumerable<MediaFile> files)
    {
        var mediaFiles = files
            .Where(f =>
                f.MainCategory.ToString() == "Image" ||
                f.MainCategory.ToString() == "Video")
            .OrderBy(f => f.CreatedAt)
            .ToList();

        var trips = new List<TripGroup>();

        if (!mediaFiles.Any())
            return trips;

        var currentTrip = new List<MediaFile>
        {
            mediaFiles.First()
        };

        for (int i = 1; i < mediaFiles.Count; i++)
        {
            var previous = mediaFiles[i - 1];
            var current = mediaFiles[i];

            var gap = current.CreatedAt - previous.CreatedAt;

            // ⭐ up to 7 days still same trip
            if (gap.TotalDays <= 7)
            {
                currentTrip.Add(current);
            }
            else
            {
                trips.Add(BuildTrip(currentTrip));
                currentTrip = new List<MediaFile> { current };
            }
        }

        if (currentTrip.Any())
            trips.Add(BuildTrip(currentTrip));

        return trips;
    }

    private TripGroup BuildTrip(List<MediaFile> files)
    {
        var start = files.Min(f => f.CreatedAt);
        var end = files.Max(f => f.CreatedAt);

        var location = GetTripLocation(files);

        return new TripGroup
        {
            Files = files,
            StartDate = start,
            EndDate = end,
            Name = $"{start:yyyy-MM-dd} to {end:yyyy-MM-dd} - {location}"
        };
    }

    private string GetTripLocation(List<MediaFile> files)
    {
        Console.WriteLine("===== TRIP LOCATION DEBUG =====");

        // ⭐ GPS ALWAYS WINS
        foreach (var file in files)
        {
            var gpsLocation = GetGpsLocation(file);

            if (!string.IsNullOrWhiteSpace(gpsLocation) &&
                gpsLocation != "Unknown")
            {
                Console.WriteLine($"[TRIP LOCATION FROM GPS] {gpsLocation}");
                return gpsLocation;
            }
        }

        // ⭐ FALLBACK TO SOURCE FOLDER
        var first = files.FirstOrDefault();

        if (first != null)
        {
            var folder = Path.GetFileName(
                Path.GetDirectoryName(first.FullPath));

            Console.WriteLine($"[FOLDER FALLBACK RAW] {folder}");

            var cleaned = Regex.Replace(
                folder ?? "",
                @"^\d{4}-\d{2}-\d{2}\s*",
                ""
            ).Trim();

            Console.WriteLine($"[FOLDER FALLBACK CLEAN] {cleaned}");

            if (!string.IsNullOrWhiteSpace(cleaned))
                return cleaned;
        }

        return "Unknown";
    }

    private string GetGpsLocation(MediaFile file)
    {
        var coords = GpsHelper.GetCoordinates(file.FullPath);

        if (coords == null)
            return "Unknown";

        var (lat, lng) = coords.Value;

        Console.WriteLine($"[GPS COORDS] {lat}, {lng}");

        // ===== DENMARK CITIES =====

        if (IsNear(lat, lng, 56.1629, 10.2039, 25))
            return "Aarhus";

        if (IsNear(lat, lng, 55.6761, 12.5683, 25))
            return "Copenhagen";

        if (IsNear(lat, lng, 55.4038, 10.4024, 20))
            return "Odense";

        if (IsNear(lat, lng, 57.0488, 9.9217, 20))
            return "Aalborg";

        if (IsNear(lat, lng, 55.4765, 8.4594, 20))
            return "Esbjerg";

        if (IsNear(lat, lng, 56.4606, 10.0364, 20))
            return "Randers";

        if (IsNear(lat, lng, 55.7093, 9.5364, 20))
            return "Vejle";

        if (IsNear(lat, lng, 55.6419, 12.0878, 20))
            return "Roskilde";

        // ===== COUNTRIES =====

        // Denmark
        if (lat >= 54.5 && lat <= 58.0 &&
            lng >= 7.5 && lng <= 15.5)
            return "Denmark";

        // Sweden
        if (lat >= 55.0 && lat <= 69.0 &&
            lng >= 11.0 && lng <= 24.5)
            return "Sweden";

        // Norway
        if (lat >= 57.0 && lat <= 71.5 &&
            lng >= 4.0 && lng <= 31.5)
            return "Norway";

        // Germany
        if (lat >= 47.0 && lat <= 55.5 &&
            lng >= 5.0 && lng <= 15.5)
            return "Germany";

        // Netherlands
        if (lat >= 50.5 && lat <= 53.8 &&
            lng >= 3.0 && lng <= 7.5)
            return "Netherlands";

        // Belgium
        if (lat >= 49.5 && lat <= 51.6 &&
            lng >= 2.5 && lng <= 6.5)
            return "Belgium";

        // France
        if (lat >= 42.0 && lat <= 51.5 &&
            lng >= -5.5 && lng <= 8.5)
            return "France";

        // Italy
        if (lat >= 36.0 && lat <= 47.5 &&
            lng >= 6.0 && lng <= 19.0)
            return "Italy";

        // Spain
        if (lat >= 36.0 && lat <= 43.8 &&
            lng >= -9.5 && lng <= 3.5)
            return "Spain";

        // Greece
        if (lat >= 34.0 && lat <= 42.0 &&
            lng >= 19.0 && lng <= 29.0)
            return "Greece";

        // Thailand
        if (lat >= 5.5 && lat <= 20.5 &&
            lng >= 97.0 && lng <= 105.6)
            return "Thailand";

        // USA
        if (lat >= 24.0 && lat <= 49.5 &&
            lng >= -125.0 && lng <= -66.0)
            return "USA";

        return "Abroad";
    }

    private bool IsNear(
        double lat1,
        double lng1,
        double lat2,
        double lng2,
        double maxKm)
    {
        const double R = 6371;

        var dLat = ToRad(lat2 - lat1);
        var dLng = ToRad(lng2 - lng1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(lat1)) *
            Math.Cos(ToRad(lat2)) *
            Math.Sin(dLng / 2) *
            Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(
            Math.Sqrt(a),
            Math.Sqrt(1 - a));

        var distance = R * c;

        return distance <= maxKm;
    }

    private double ToRad(double angle)
    {
        return angle * Math.PI / 180;
    }
}