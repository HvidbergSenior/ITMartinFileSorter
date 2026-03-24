using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public class TripGroupingService
{
    private const double DistanceThresholdKm = 80;
    private const int TimeGapHours = 48;

    public List<TripGroup> CreateTrips(IEnumerable<MediaFile> files)
    {
        var candidates = files
            .Select(f => new
            {
                File = f,
                Date = ImageMetadataHelper.GetCreationTime(f.FullPath) ?? f.CreatedAt,
                Coords = GpsHelper.GetCoordinates(f.FullPath)
            })
            .Where(x => x.Coords != null)
            .OrderBy(x => x.Date)
            .ToList();

        var trips = new List<TripGroup>();

        TripGroup? current = null;
        DateTime? lastDate = null;
        (double lat, double lng)? lastCoords = null;

        foreach (var item in candidates)
        {
            bool newTrip = false;

            if (current == null)
            {
                newTrip = true;
            }
            else
            {
                var timeGap = (item.Date - lastDate!.Value).TotalHours;

                var distance = DistanceKm(
                    lastCoords!.Value.lat,
                    lastCoords.Value.lng,
                    item.Coords!.Value.lat,
                    item.Coords.Value.lng);

                if (timeGap > TimeGapHours || distance > DistanceThresholdKm)
                    newTrip = true;
            }

            if (newTrip)
            {
                current = new TripGroup
                {
                    StartDate = item.Date,
                    EndDate = item.Date
                };

                trips.Add(current);
            }

            current!.Files.Add(item.File);
            current.EndDate = item.Date;

            lastDate = item.Date;
            lastCoords = item.Coords;
        }

        // Name trips
        int tripNumber = 1;

        foreach (var trip in trips)
        {
            trip.Name = $"Trip_{trip.StartDate:yyyy-MM-dd}_{tripNumber}";
            tripNumber++;
        }

        return trips;
    }

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;

        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a =
            Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
            Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }

    private static double ToRad(double angle) => angle * Math.PI / 180;
}