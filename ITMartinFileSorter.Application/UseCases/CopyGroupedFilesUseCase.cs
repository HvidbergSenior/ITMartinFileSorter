using ITMartinFileScanner.Domain.Entities;
using ITMartinFileScanner.Domain.Interfaces;

namespace ITMartinFileScanner.Application.UseCases;

public class CopyGroupedFilesUseCase
{
    private readonly IFileCopyRepository _repo;

    public CopyGroupedFilesUseCase(IFileCopyRepository repo)
    {
        _repo = repo;
    }

    public void Execute(IEnumerable<MediaFile> files, string outputRoot)
    {
        foreach (var file in files)
        {
            string targetFolder;

            if (!string.IsNullOrWhiteSpace(file.DynamicFolder))
            {
                // Example: Keep/Videos/2023
                targetFolder = Path.Combine(
                    outputRoot,
                    file.MainCategory.ToString(),
                    file.DynamicFolder
                );
            }
            else
            {
                // Default: Keep/Screenshot etc.
                targetFolder = Path.Combine(
                    outputRoot,
                    file.MainCategory.ToString(),
                    file.SubCategory.ToString()
                );
            }

            _repo.CreateDirectory(targetFolder);

            var destFile = GetUniquePath(
                Path.Combine(targetFolder, Path.GetFileName(file.FullPath))
            );

            _repo.Copy(file.FullPath, destFile);
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