using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public static class FileNameBuilder
{
    public static string Build(MediaFile file, int index)
    {
        var ext = Path.GetExtension(file.FileName);

        var timestamp = file.CreatedAt
            .ToString("yyyy-MM-dd HH-mm-ss");

        return $"{timestamp}_{index:D3}{ext}";
    }
}