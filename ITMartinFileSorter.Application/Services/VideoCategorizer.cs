using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class VideoCategorizer
{
    public void Categorize(MediaFile file)
    {
        var name = file.FileName.ToLowerInvariant();
        string yearMonth = file.Year > 1990
            ? $"{file.Year}-{file.Month:00}"
            : "Unknown";

        if (name.Contains("screen") || name.Contains("capture"))
        {
            file.SubCategory = MediaSubCategory.ScreenRecording;
        }
        else if (name.StartsWith("vid_") ||
                 name.StartsWith("mov_") ||
                 name.StartsWith("img_") ||
                 name.StartsWith("pxl_"))
        {
            file.SubCategory = MediaSubCategory.PhoneVideo;
        }
        else
        {
            file.SubCategory = MediaSubCategory.OtherVideo;
        }

        file.DynamicFolder = Path.Combine(
            "Videos",
            file.SubCategory.ToString(),
            yearMonth);
    }
}