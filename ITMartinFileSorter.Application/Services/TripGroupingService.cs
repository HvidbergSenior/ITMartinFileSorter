using System.Globalization;
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
            Name = BuildTripFolderName(start, end, location)
        };
    }

    private string BuildTripFolderName(
        DateTime start,
        DateTime end,
        string location)
    {
        var culture = new CultureInfo("da-DK");

        var startMonth = start.ToString("MMMM", culture);
        var endMonth = end.ToString("MMMM", culture);

        startMonth =
            char.ToUpper(startMonth[0]) +
            startMonth[1..];

        endMonth =
            char.ToUpper(endMonth[0]) +
            endMonth[1..];

        var datePart =
            $"{start.Day}{startMonth}-{end.Day}{endMonth}";

        if (!string.IsNullOrWhiteSpace(location) &&
            location != "Unknown")
        {
            return $"{datePart} {location}";
        }

        return datePart;
    }

    private string GetTripLocation(List<MediaFile> files)
    {
        Console.WriteLine("===== TRIP LOCATION DEBUG =====");

        foreach (var file in files)
        {
            var gpsLocation = GetGpsLocation(file);

            if (!string.IsNullOrWhiteSpace(gpsLocation) &&
                gpsLocation != "Unknown")
            {
                Console.WriteLine(
                    $"[TRIP LOCATION FROM GPS] {gpsLocation}");

                return gpsLocation;
            }
        }

        var first = files.FirstOrDefault();

        if (first != null)
        {
            var folder = Path.GetFileName(
                Path.GetDirectoryName(first.FullPath));

            Console.WriteLine(
                $"[FOLDER FALLBACK RAW] {folder}");

            var cleaned = Regex.Replace(
                folder ?? "",
                @"^\d{4}-\d{2}-\d{2}\s*",
                "")
                .Trim();

            Console.WriteLine(
                $"[FOLDER FALLBACK CLEAN] {cleaned}");

            if (!string.IsNullOrWhiteSpace(cleaned))
                return cleaned;
        }

        return "Unknown";
    }

    private string GetGpsLocation(MediaFile file)
    {
        var coords = GpsHelper.GetCoordinates(file.FullPath);

        Console.WriteLine("===== GPS DEBUG =====");
        Console.WriteLine($"[FILE] {file.FileName}");
        Console.WriteLine($"[PATH] {file.FullPath}");

        if (coords == null)
        {
            Console.WriteLine("[NO GPS FOUND]");
            return "Unknown";
        }

        var (lat, lng) = coords.Value;

        Console.WriteLine($"[LAT] {lat}");
        Console.WriteLine($"[LNG] {lng}");

        // ===== DENMARK =====
        if (IsNear(lat, lng, 56.1629, 10.2039, 25))
        {
            Console.WriteLine("[MATCH] Aarhus");
            return "Aarhus";
        }

        if (IsNear(lat, lng, 55.6761, 12.5683, 25))
        {
            Console.WriteLine("[MATCH] Copenhagen");
            return "Copenhagen";
        }

        if (IsNear(lat, lng, 57.0488, 9.9217, 25))
        {
            Console.WriteLine("[MATCH] Aalborg");
            return "Aalborg";
        }

        // ===== FRANCE =====
        if (lat >= 42.0 && lat <= 51.5 &&
            lng >= -5.5 && lng <= 8.5)
        {
            Console.WriteLine("[MATCH] France");
            return "France";
        }

        // ===== AUSTRIA =====
        if (lat >= 46.0 && lat <= 49.2 &&
            lng >= 9.0 && lng <= 17.5)
        {
            Console.WriteLine("[MATCH] Austria");
            return "Austria";
        }

        // ===== THAILAND =====
        if (lat >= 5.5 && lat <= 20.5 &&
            lng >= 97.0 && lng <= 105.6)
        {
            Console.WriteLine("[MATCH] Thailand");
            return "Thailand";
        }

        Console.WriteLine("[MATCH] Abroad");
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

    public string GetSingleFileLocation(MediaFile file)
    {
        return GetGpsLocation(file);
    }
}