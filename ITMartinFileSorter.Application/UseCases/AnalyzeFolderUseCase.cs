using ITMartinFileScanner.Domain.Entities;
using ITMartinFileScanner.Domain.Enums;
using ITMartinFileScanner.Domain.Interfaces;

namespace ITMartinFileScanner.Application.UseCases;

public class AnalyzeFolderUseCase
{
    private readonly IFileScanner _scanner;
    private readonly IHashService _hashService;

    public AnalyzeFolderUseCase(IFileScanner scanner, IHashService hashService)
    {
        _scanner = scanner;
        _hashService = hashService;
    }

    public List<MediaFile> Execute(string folderPath)
    {
        var files = _scanner.ScanFolder(folderPath).ToList();

        foreach (var file in files)
        {
            // Hash (exact duplicates)
            var hash = _hashService.ComputeHash(file.FullPath);
            file.SetHash(hash);

            // Video metadata (for duplicate detection)
            if (file.Type == MediaType.Video)
            {
                try
                {
                    var tag = TagLib.File.Create(file.FullPath);

                    var durationMs = (long?)tag.Properties.Duration.TotalMilliseconds;
                    var width = tag.Properties.VideoWidth;
                    var height = tag.Properties.VideoHeight;

                    file.SetVideoMetadata(durationMs, width, height);
                }
                catch
                {
                    file.SetVideoMetadata(null, null, null);
                }
            }
        }

        return files;
    }
}