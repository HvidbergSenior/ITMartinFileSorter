using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public class EventDetectionService
{
    public List<List<MediaFile>> DetectEvents(IEnumerable<MediaFile> files)
    {
        var sorted = files
            .OrderBy(f => f.CreatedAt)
            .ToList();

        var events = new List<List<MediaFile>>();
        List<MediaFile>? current = null;

        DateTime? last = null;

        foreach (var file in sorted)
        {
            if (current == null)
            {
                current = new List<MediaFile>();
                events.Add(current);
            }
            else if ((file.CreatedAt - last!.Value).TotalHours > 6)
            {
                current = new List<MediaFile>();
                events.Add(current);
            }

            current.Add(file);
            last = file.CreatedAt;
        }

        return events.Where(e => e.Count > 10).ToList();
    }
}