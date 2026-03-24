using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public static class GroupKeyResolver
{
    public static string GetGroupKey(MediaFile file, GroupingOptions options)
    {
        return options.Strategy switch
        {
            GroupingStrategy.ByDate => GetDateGroup(file),
            GroupingStrategy.ByLocation => GetLocationGroup(file),
            GroupingStrategy.ByDevice => GetDeviceGroup(file),
            GroupingStrategy.ByMediaType => file.MainCategory.ToString(),
            GroupingStrategy.CustomPrefix => options.CustomPrefix ?? "Files",
            _ => "Files"
        };
    }

    private static string GetDateGroup(MediaFile file)
    {
        if (file.Year > 0 && file.Month > 0)
            return $"{file.Year}-{file.Month:D2}";

        return "UnknownDate";
    }

    private static string GetLocationGroup(MediaFile file)
    {
        var coords = GpsHelper.GetCoordinates(file.FullPath);

        if (coords == null)
            return "UnknownLocation";

        var (lat, lng) = coords.Value;

        if (LocationFilter.IsInAarhus(lat, lng)) return "Aarhus";
        if (LocationFilter.IsInDenmark(lat, lng)) return "Denmark";
        if (LocationFilter.IsInSweden(lat, lng)) return "Sweden";

        if (lat >= 5.5 && lat <= 20.5 && lng >= 97.3 && lng <= 105.6)
            return "Thailand";

        return "Abroad";
    }

    private static string GetDeviceGroup(MediaFile file)
    {
        if (ExifHelper.IsIphone(file.FullPath)) return "iPhone";
        if (ExifHelper.IsAndroid(file.FullPath)) return "Android";

        return "Camera";
    }
}