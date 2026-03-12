using ITMartinFileScanner.Domain.Entities;
using ITMartinFileScanner.Domain.Enums;
using ITMartinFileScanner.Domain.Interfaces;

namespace ITMartinFileScanner.Application.UseCases;

public class CopyDuplicateGroupsUseCase
{
    private readonly IFileCopyRepository _repo;

    public CopyDuplicateGroupsUseCase(IFileCopyRepository repo)
    {
        _repo = repo;
    }

    public void Execute(
        Dictionary<string, List<MediaFile>> duplicates,
        string outputRoot)
    {
        var dupRoot = Path.Combine(outputRoot, MediaMainCategory.DeleteCandidate.ToString(),
            MediaSubCategory.Duplicate.ToString());
        _repo.CreateDirectory(dupRoot);

        int groupIndex = 1;

        foreach (var group in duplicates)
        {
            var groupFolder = Path.Combine(dupRoot, $"Group_{groupIndex}");
            _repo.CreateDirectory(groupFolder);

            foreach (var file in group.Value)
            {
                var destFile = GetUniquePath(Path.Combine(groupFolder, Path.GetFileName(file.FullPath)));
                _repo.Copy(file.FullPath, destFile);
            }

            groupIndex++;
        }
    }

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