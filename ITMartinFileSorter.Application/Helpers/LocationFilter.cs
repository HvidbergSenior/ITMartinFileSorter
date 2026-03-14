namespace ITMartinFileSorter.Application.Helpers;

public static class LocationFilter
{
    private const double AarhusLat = 56.1629;
    private const double AarhusLng = 10.2039;
    private const double AarhusRadiusKm = 20;

    public static bool IsInAarhus(double lat, double lng) => DistanceKm(lat, lng, AarhusLat, AarhusLng) <= AarhusRadiusKm;
    public static bool IsInSweden(double lat, double lng) => lat >= 55.3 && lat <= 69.1 && lng >= 11.0 && lng <= 24.2;
    public static bool IsInDenmark(double lat, double lng) => lat >= 54.5 && lat <= 58.5 && lng >= 7.5 && lng <= 15.5;
    public static bool IsInSjaelland(double lat, double lng) => lat >= 55.2 && lat <= 56.2 && lng >= 11.3 && lng <= 12.7;
    public static bool IsInJutlandMinusAarhus(double lat, double lng) => IsInDenmark(lat, lng) && !IsInSjaelland(lat, lng) && !IsInAarhus(lat, lng);
    public static bool IsOutsideDenmarkAndSweden(double lat, double lng) => !IsInDenmark(lat, lng) && !IsInSweden(lat, lng);

    private static double DistanceKm(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRad(double angle) => angle * Math.PI / 180;
}