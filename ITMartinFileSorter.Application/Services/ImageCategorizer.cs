using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class ImageCategorizer
{
    public void Categorize(MediaFile file)
    {
        var ext = file.Extension.ToLowerInvariant();
        var name = file.FileName.ToLowerInvariant();

        string yearMonth = file.Year > 1990
            ? $"{file.Year}-{file.Month:00}"
            : "Unknown";

        if (name.Contains("screenshot"))
            file.SubCategory = MediaSubCategory.Screenshot;

        else if (ext is ".heic" or ".heif")
            file.SubCategory = MediaSubCategory.PhonePhoto;

        else if (name.StartsWith("img_") || name.StartsWith("dsc_"))
            file.SubCategory = MediaSubCategory.Camera;

        else
            file.SubCategory = MediaSubCategory.OtherImage;

        // Keep this FAST (no EXIF here yet)
        file.DynamicFolder = Path.Combine(
            "Images",
            file.SubCategory.ToString(),
            yearMonth
        );
    }
}