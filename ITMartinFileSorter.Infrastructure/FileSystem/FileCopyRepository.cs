using ITMartinFileSorter.Domain.Interfaces;

namespace ITMartinFileSorter.Infrastructure.FileSystem;

public class FileCopyRepository : IFileCopyRepository
{
    public void Copy(string sourcePath, string destinationPath)
    {
        File.Copy(sourcePath, destinationPath, overwrite: false);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}