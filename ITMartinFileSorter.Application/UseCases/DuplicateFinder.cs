using ITMartinFileScanner.Domain.Entities;
using ITMartinFileScanner.Domain.Enums;

namespace ITMartinFileScanner.Application.UseCases;

public class DuplicateFinder
{
    public Dictionary<string, List<MediaFile>> FindDuplicates(
        IEnumerable<MediaFile> files,
        DateTime? from = null,
        DateTime? to = null)
    {
        var filtered = FilterByDate(files, from, to);

        var result = new Dictionary<string, List<MediaFile>>();

        // 1) exact hash duplicates
        foreach (var group in filtered
                     .Where(f => !string.IsNullOrEmpty(f.Hash))
                     .GroupBy(f => f.Hash)
                     .Where(g => g.Count() > 1))
        {
            result[$"HASH:{group.Key}"] = group.ToList();
        }

        // 2) video metadata duplicates (duration + resolution)
        var videoGroups = filtered
            .Where(f => f.Type == MediaType.Video)
            .Where(f => f.DurationMs.HasValue && f.Width.HasValue && f.Height.HasValue)
            .GroupBy(f => new { f.DurationMs, f.Width, f.Height });

        foreach (var group in videoGroups.Where(g => g.Count() > 1))
        {
            string key = $"META:{group.Key.DurationMs}-{group.Key.Width}x{group.Key.Height}";
            result[key] = group.ToList();
        }

        return result;
    }

    private IEnumerable<MediaFile> FilterByDate(
        IEnumerable<MediaFile> files,
        DateTime? from,
        DateTime? to)
    {
        if (from == null && to == null)
            return files;

        return files.Where(f =>
        {
            var dt = f.CreatedAt;

            if (from.HasValue && dt < from.Value)
                return false;

            if (to.HasValue && dt > to.Value)
                return false;

            return true;
        });
    }
}