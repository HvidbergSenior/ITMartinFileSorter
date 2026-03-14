using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Application.Models;

namespace ITMartinFileSorter.Application.Services;

public static class DuplicateGrouper
{
    public static List<DuplicateGroup> CreateGroups(List<MediaFile> files)
    {
        return files
            .Where(f => f.SubCategory.ToString() == "Duplicate")
            .GroupBy(f => f.SizeBytes)
            .Select(g => new DuplicateGroup
            {
                Files = g.ToList()
            })
            .Where(g => g.Files.Count > 1)
            .ToList();
    }
}