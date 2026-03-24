using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class DuplicateService
{
    public string FolderPath { get; set; } = "";

    public List<MediaFile> AllFiles { get; set; } = new();

    public List<List<MediaFile>> DuplicateGroups { get; set; } = new();

    // Track if duplicates were handled
    public bool DuplicatesHandled { get; set; } = false;

    public HashSet<MediaMainCategory> CompletedCategories { get; set; } = new();

    // ⭐ NEW: Grouping choice from user
    public GroupingOptions? GroupingOptions { get; set; }

    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();

    // ⭐ Helper: Files selected for export
    public IEnumerable<MediaFile> FilesToExport =>
        AllFiles.Where(f => f.Status == MediaFileStatus.ToKeep);

    // ⭐ Reset state when scanning new folder
    public void Reset()
    {
        AllFiles.Clear();
        DuplicateGroups.Clear();
        CompletedCategories.Clear();
        GroupingOptions = null;
        DuplicatesHandled = false;
        FolderPath = "";

        NotifyStateChanged();
    }

    // ⭐ Debug helper
    public void PrintFiles(string header = "AllFiles")
    {
        Console.WriteLine($"[DEBUG] {header} ({AllFiles.Count} files):");

        foreach (var f in AllFiles)
            Console.WriteLine($"    {f.FileName} | {f.MainCategory} | {f.Status}");
    }
}