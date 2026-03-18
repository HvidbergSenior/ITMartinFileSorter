using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public class DuplicateService
{
    public string FolderPath { get; set; } = "";
    public string WorkingFolder { get; set; } = "";
    public List<MediaFile> AllFiles { get; set; } = new();
    public List<List<MediaFile>> DuplicateGroups { get; set; } = new();

    // Count of duplicates automatically removed
    public int DuplicatesRemoved { get; set; } = 0;

    // NEW: track if duplicates were handled
    public bool DuplicatesHandled { get; set; } = false;
    
    // Event to trigger UI updates
    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();
}