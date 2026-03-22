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
    public event Action? OnChange;

    public void NotifyStateChanged() => OnChange?.Invoke();

    // Debug helper
    public void PrintFiles(string header = "AllFiles")
    {
        Console.WriteLine($"[DEBUG] {header} ({AllFiles.Count} files):");
        foreach (var f in AllFiles)
            Console.WriteLine($"    {f}");
    }
}