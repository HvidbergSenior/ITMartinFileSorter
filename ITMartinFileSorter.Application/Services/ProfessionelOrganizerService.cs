using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class ProfessionalOrganizerService
{
    private readonly TripGroupingService _tripService;
    private readonly HomeLocationService _homeService;
    private readonly JunkDetectionService _junkService;

    public ProfessionalOrganizerService(
        TripGroupingService tripService,
        HomeLocationService homeService,
        JunkDetectionService junkService)
    {
        _tripService = tripService;
        _homeService = homeService;
        _junkService = junkService;
    }

    public Dictionary<string, List<MediaFile>> BuildArchive(
        IEnumerable<MediaFile> files,
        OrganizerOptions options)
    {
        var result = new Dictionary<string, List<MediaFile>>();

        var home = options.DetectHome
            ? _homeService.DetectHome(files)
            : null;

        var trips = options.DetectTrips
            ? _tripService.CreateTrips(files)
            : new();

        foreach (var file in files)
        {
            if (options.RemoveScreenshots &&
                _junkService.IsScreenshot(file))
            {
                Add(result, Path.Combine("Junk", "Screenshots"), file);
                continue;
            }

            var trip = trips.FirstOrDefault(t => t.Files.Contains(file));

            if (trip != null)
            {
                Add(result,
                    Path.Combine("Organized",
                        GetCategoryRoot(file),
                        "Trips",
                        trip.Name),
                    file);

                continue;
            }

            if (home != null)
            {
                var coords = GpsHelper.GetCoordinates(file.FullPath);

                if (coords != null &&
                    _homeService.IsNearHome(home.Value, coords.Value))
                {
                    Add(result,
                        Path.Combine("Organized",
                            GetCategoryRoot(file),
                            "Home"),
                        file);

                    continue;
                }
            }

            Add(result,
                Path.Combine("Organized",
                    GetCategoryRoot(file),
                    "By Year",
                    file.Year.ToString(),
                    $"{file.Year}-{file.Month:D2}"),
                file);
        }

        return result;
    }

    private string GetCategoryRoot(MediaFile file)
    {
        return file.MainCategory switch
        {
            MediaMainCategory.Image => "Photos",
            MediaMainCategory.Video => "Videos",
            MediaMainCategory.Document => "Documents",
            MediaMainCategory.Audio => "Audio",
            _ => "Other"
        };
    }

    private void Add(
        Dictionary<string, List<MediaFile>> dict,
        string path,
        MediaFile file)
    {
        if (!dict.ContainsKey(path))
            dict[path] = new List<MediaFile>();

        dict[path].Add(file);
    }
}