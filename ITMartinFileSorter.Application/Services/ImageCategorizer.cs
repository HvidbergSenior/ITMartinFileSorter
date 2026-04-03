using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class ImageCategorizer
{
    public void Categorize(MediaFile file)
    {
        var name = file.FileName.ToLowerInvariant();
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        string yearMonth = file.Year > 1990
            ? $"{file.Year}-{file.Month:00}"
            : "Unknown";

        if (IsScreenshot(file, name, ext))
        {
            file.SubCategory = MediaSubCategory.Screenshot;
        }
        else if (IsMeme(file, name, ext))
        {
            file.SubCategory = MediaSubCategory.Meme;
        }
        else if (IsSocial(file, name))
        {
            file.SubCategory = MediaSubCategory.Social;
        }
        else if (IsCameraPhoto(name, ext))
        {
            file.SubCategory = MediaSubCategory.Camera;
        }
        else
        {
            file.SubCategory = MediaSubCategory.OtherImage;
        }

        file.DynamicFolder = file.SubCategory switch
        {
            MediaSubCategory.Screenshot => Path.Combine(
                "Screenshots",
                yearMonth),

            MediaSubCategory.Meme => Path.Combine(
                "Memes",
                yearMonth),

            _ => Path.Combine(
                "Images",
                yearMonth)
        };
    }

    private bool IsScreenshot(MediaFile file, string name, string ext)
    {
        // All PNG phone captures / screenshots
        if (ext == ".png")
            return true;

        if (name.Contains("screenshot") ||
            name.Contains("screen") ||
            name.Contains("capture"))
            return true;

        return false;
    }

    private bool IsMeme(MediaFile file, string name, string ext)
    {
        // All gif / webp files are memes / reactions
        if (ext == ".gif" || ext == ".webp")
            return true;

        if (name.Contains("meme") ||
            name.Contains("funny") ||
            name.Contains("reaction") ||
            name.Contains("sticker") ||
            name.Contains("joker"))
            return true;

        return false;
    }

    private bool IsSocial(MediaFile file, string name)
    {
        return name.Contains("facebook") ||
               name.Contains("messenger") ||
               name.Contains("whatsapp") ||
               name.Contains("instagram") ||
               name.Contains("snapchat");
    }

    private bool IsCameraPhoto(string name, string ext)
    {
        // iPhone / camera formats
        if (ext == ".heic" ||
            ext == ".jpg" ||
            ext == ".jpeg")
            return true;

        return name.StartsWith("img_") ||
               name.StartsWith("pxl_") ||
               name.StartsWith("dsc_");
    }
}