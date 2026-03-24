using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class ArchivePathBuilder
{
    private readonly TripGroupingService _tripService;

    public ArchivePathBuilder(TripGroupingService tripService)
    {
        _tripService = tripService;
    }

    public Dictionary<string, List<MediaFile>> BuildStructure(
        IEnumerable<MediaFile> files,
        ArchiveOptions options)
    {
        var result = new Dictionary<string, List<MediaFile>>();

        var trips = options.UseTripFolders
            ? _tripService.CreateTrips(files)
            : new();

        foreach (var file in files)
        {
            var path = BuildPath(file, trips, options);

            if (!result.ContainsKey(path))
                result[path] = new List<MediaFile>();

            result[path].Add(file);
        }

        return result;
    }

    private string BuildPath(
        MediaFile file,
        List<TripGroup> trips,
        ArchiveOptions options)
    {
        var parts = new List<string> { "Organized" };

        switch (file.MainCategory)
        {
            case MediaMainCategory.Image:
                parts.Add("Photos");
                AddPhotoSubfolders(file, trips, options, parts);
                break;

            case MediaMainCategory.Video:
                parts.Add("Videos");
                AddVideoSubfolders(file, trips, options, parts);
                break;

            case MediaMainCategory.Document:
                parts.Add("Documents");
                break;

            case MediaMainCategory.Audio:
                parts.Add("Audio");
                break;
        }

        return Path.Combine(parts.ToArray());
    }

    private void AddPhotoSubfolders(
        MediaFile file,
        List<TripGroup> trips,
        ArchiveOptions options,
        List<string> parts)
    {
        var trip = trips.FirstOrDefault(t => t.Files.Contains(file));

        if (trip != null)
        {
            parts.Add("Trips");
            parts.Add(trip.Name);
            return;
        }

        if (options.UseYearFolders)
        {
            parts.Add("By Year");
            parts.Add(file.Year.ToString());
            parts.Add($"{file.Year}-{file.Month:D2}");
        }
    }

    private void AddVideoSubfolders(
        MediaFile file,
        List<TripGroup> trips,
        ArchiveOptions options,
        List<string> parts)
    {
        var trip = trips.FirstOrDefault(t => t.Files.Contains(file));

        if (trip != null)
        {
            parts.Add("Trips");
            parts.Add(trip.Name);
            return;
        }

        parts.Add("Home Videos");

        if (options.UseYearFolders)
        {
            parts.Add(file.Year.ToString());
            parts.Add($"{file.Year}-{file.Month:D2}");
        }
    }
}