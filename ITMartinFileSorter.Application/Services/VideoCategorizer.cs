using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class VideoCategorizer
{
    public void Categorize(MediaFile file)
    {
        var name = file.FileName.ToLowerInvariant();
        string yearMonth = file.Year > 1990 ? $"{file.Year}-{file.Month:00}" : "Unknown";

        if (name.Contains("screen"))
            file.SubCategory = MediaSubCategory.ScreenRecording;
        else if (name.StartsWith("img_"))
            file.SubCategory = MediaSubCategory.PhoneVideo;
        else
            file.SubCategory = MediaSubCategory.OtherVideo;

        file.DynamicFolder = Path.Combine("Videos", file.SubCategory.ToString(), yearMonth);

        Console.WriteLine($"[DEBUG] Categorized {file.FileName} -> Main: {file.MainCategory}, Sub: {file.SubCategory}, DynamicFolder: {file.DynamicFolder}");
    }
}