using ITMartinFileSorter.Application.Helpers;
using ITMartinFileSorter.Domain.Entities;

namespace ITMartinFileSorter.Application.Services;

public static class SmartRenameService
{
    /// <summary>
    /// Builds a new filename based on metadata and grouping options.
    /// </summary>
    public static string BuildNewName(
        MediaFile file,
        int index,
        GroupingOptions options)
    {
        // 🔒 If renaming disabled → keep original name
        if (!options.RenameFiles)
            return file.FileName;

        var path = file.FullPath;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        // ⭐ 1. Best available date
        var date =
            ImageMetadataHelper.GetCreationTime(path) ??
            VideoMetadataHelper.GetCreationTime(path) ??
            file.CreatedAt;

        // ⭐ 2. Device detection
        string device =
            ExifHelper.IsIphone(path) ? "iPhone" :
            ExifHelper.IsAndroid(path) ? "Android" :
            "Camera";

        // ⭐ 3. Group name (location/date/etc.)
        string group = GroupKeyResolver.GetGroupKey(file, options);

        if (string.IsNullOrWhiteSpace(group))
            group = "Files";

        // ⭐ 4. Final filename
        return $"{date:yyyy-MM-dd_HH-mm-ss}_{group}_{device}_{index:D3}{ext}";
    }

    /// <summary>
    /// Ensures the filename is unique within the destination folder.
    /// </summary>
    public static string EnsureUnique(
        string folder,
        string fileName)
    {
        string fullPath = Path.Combine(folder, fileName);

        if (!File.Exists(fullPath))
            return fileName;

        string name = Path.GetFileNameWithoutExtension(fileName);
        string ext = Path.GetExtension(fileName);

        int i = 1;

        while (true)
        {
            string newName = $"{name}_{i}{ext}";
            string newPath = Path.Combine(folder, newName);

            if (!File.Exists(newPath))
                return newName;

            i++;
        }
    }
}