using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;
using ITMartinFileSorter.Domain.Interfaces;

namespace ITMartinFileSorter.Application.UseCases;

public class CopyDuplicateGroupsUseCase
{
    private readonly IFileCopyRepository _repo;

    public CopyDuplicateGroupsUseCase(IFileCopyRepository repo)
    {
        _repo = repo;
    }

    /// <summary>
    /// Copies grouped duplicate files into a structured folder under "Duplicates".
    /// </summary>
    /// <param name="duplicates">Dictionary of hash → list of duplicate MediaFiles</param>
    /// <param name="outputRoot">Root folder for copied duplicates</param>
    public void Execute(
        Dictionary<string, List<MediaFile>> duplicates,
        string outputRoot)
    {
        // Use a generic "Duplicates" folder under output root
        var dupRoot = Path.Combine(outputRoot, "Duplicates");
        _repo.CreateDirectory(dupRoot);

        int groupIndex = 1;

        foreach (var group in duplicates)
        {
            var groupFolder = Path.Combine(dupRoot, $"Group_{groupIndex}");
            _repo.CreateDirectory(groupFolder);

            foreach (var file in group.Value)
            {
                // Keep original file name but ensure uniqueness
                var destFile = GetUniquePath(Path.Combine(groupFolder, Path.GetFileName(file.FullPath)));
                _repo.Copy(file.FullPath, destFile);
            }

            groupIndex++;
        }
    }

    /// <summary>
    /// Generates a unique file path to prevent overwriting existing files
    /// </summary>
    private string GetUniquePath(string path)
    {
        if (!File.Exists(path))
            return path;

        var dir = Path.GetDirectoryName(path)!;
        var name = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        int i = 1;
        while (true)
        {
            var newPath = Path.Combine(dir, $"{name}_{i}{ext}");
            if (!File.Exists(newPath))
                return newPath;
            i++;
        }
    }
}
