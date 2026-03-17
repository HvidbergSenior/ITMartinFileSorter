using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public class DuplicateService
{
    public string FolderPath { get; set; } = "";
    public string WorkingFolder { get; set; } = "";
    public List<MediaFile> AllFiles { get; set; } = new();
    public List<List<MediaFile>> DuplicateGroups { get; set; } = new();

    // NEW: count of duplicates automatically removed
    public int DuplicatesRemoved { get; set; } = 0;
}