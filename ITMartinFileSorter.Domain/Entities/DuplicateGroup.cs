using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Models;

public class DuplicateGroup
{
    public List<MediaFile> Files { get; set; } = new();

    public MediaFile Newest
    {
        get
        {
            return Files
                .OrderByDescending(f => f.CreatedAt)
                .First();
        }
    }
}