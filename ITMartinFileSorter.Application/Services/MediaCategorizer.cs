using ITMartinFileScanner.Application.Helpers;
using ITMartinFileScanner.Domain.Entities;
using ITMartinFileScanner.Domain.Enums;

namespace ITMartinFileScanner.Application.Services;

public class MediaCategorizer
{
    public void Categorize(MediaFile file)
    {
        Console.WriteLine($"Categorize: {file.FullPath} | Created: {file.CreatedAt}");

        var lowerName = Path.GetFileName(file.FullPath).ToLowerInvariant();
        var ext = Path.GetExtension(file.FullPath).Trim().ToLowerInvariant();

        // ----------------------------
        // 📅 Year-Month (yyyy-MM)
        // ----------------------------
        string yearMonth = file.CreatedAt.Year > 1990
            ? file.CreatedAt.ToString("yyyy-MM")
            : "Unknown";

        string yearOnly = file.CreatedAt.Year > 1990
            ? file.CreatedAt.Year.ToString()
            : "Unknown";

        // ----------------------------
        // 📱 Device detection (EXIF)
        // ----------------------------
        if (file.Type == MediaType.Image)
        {
            var meta = ExifHelper.ReadMetadata(file.FullPath);

            if (meta.HasValue)
            {
                var (make, model, software) = meta.Value;

                if (make?.Contains("Apple", StringComparison.OrdinalIgnoreCase) == true ||
                    model?.Contains("iPhone", StringComparison.OrdinalIgnoreCase) == true ||
                    software?.Contains("Apple", StringComparison.OrdinalIgnoreCase) == true)
                {
                    file.SubCategory = MediaSubCategory.Iphone;
                }
                else if (!string.IsNullOrWhiteSpace(make))
                {
                    file.SubCategory = MediaSubCategory.Android;
                }
            }
        }

        // ----------------------------
        // 📱 Screenshots
        // ----------------------------
        if (file.Type == MediaType.Image && ext == ".png")
        {
            var coordinates = GpsHelper.GetCoordinates(file.FullPath);

            bool looksLikeScreenshot =
                lowerName.Contains("screenshot") ||
                lowerName.Contains("screen_") ||
                lowerName.Contains("skærm") ||
                !coordinates.HasValue;

            if (looksLikeScreenshot)
            {
                file.MainCategory = MediaMainCategory.Keep;
                file.SubCategory = MediaSubCategory.Screenshot;
                return;
            }
        }

        // ----------------------------
        // 🖼️ Images → Model / yyyy-MM / Location
        // ----------------------------
        if (file.Type == MediaType.Image)
        {
            if (ImageQualityHelper.IsBlurry(file.FullPath))
            {
                file.MainCategory = MediaMainCategory.Review;
                file.SubCategory = MediaSubCategory.Blurry;
                return;
            }

            var meta = ExifHelper.ReadMetadata(file.FullPath);
            string model = meta?.Model ?? "Unknown";
            model = model.Replace(" ", "_");

            string locationFolder = "UnknownLocation";

            var coordinates = GpsHelper.GetCoordinates(file.FullPath);
            if (coordinates.HasValue)
            {
                var (lat, lng) = coordinates.Value;

                if (LocationFilter.IsInAarhus(lat, lng))
                    locationFolder = "Aarhus";
                else if (LocationFilter.IsInSweden(lat, lng))
                    locationFolder = "Sweden";
                else if (LocationFilter.IsInJutlandMinusAarhus(lat, lng))
                    locationFolder = "Jutland";
                else if (LocationFilter.IsInSjaelland(lat, lng))
                    locationFolder = "Sjaelland";
                else if (LocationFilter.IsOutsideDenmarkAndSweden(lat, lng))
                    locationFolder = "RestOfWorld";
            }

            file.MainCategory = MediaMainCategory.Keep;
            file.SubCategory = file.SubCategory;

            file.DynamicFolder = Path.Combine(
                "Images",
                "Models",
                model,
                yearMonth,
                locationFolder);

            Console.WriteLine($"Image folder: {file.DynamicFolder}");
            return;
        }

        // ----------------------------
        // 🎥 Videos → yyyy-MM
        // ----------------------------
        if (file.Type == MediaType.Video)
        {
            file.MainCategory = MediaMainCategory.Keep;
            file.SubCategory = MediaSubCategory.IphoneVideo;

            file.DynamicFolder = Path.Combine("Videos", yearMonth);

            Console.WriteLine($"Video folder: {file.DynamicFolder}");
            return;
        }

        // ----------------------------
        // 🎵 Music → Year
        // ----------------------------
        if (file.Type == MediaType.Audio)
        {
            file.MainCategory = MediaMainCategory.Keep;
            file.SubCategory = MediaSubCategory.Unknown;

            file.DynamicFolder = Path.Combine("Music", yearOnly);

            Console.WriteLine($"Music folder: {file.DynamicFolder}");
            return;
        }

        // ----------------------------
        // 📄 Documents → Extension
        // ----------------------------
        if (file.Type == MediaType.Document)
        {
            file.MainCategory = MediaMainCategory.Keep;
            file.SubCategory = MediaSubCategory.Unknown;

            string extension = Path.GetExtension(file.FullPath)
                .TrimStart('.')
                .ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(extension))
                extension = "Unknown";

            file.DynamicFolder = Path.Combine("Documents", extension);

            Console.WriteLine($"Document folder: {file.DynamicFolder}");
            return;
        }

        // ----------------------------
        // 🧐 Everything else → Review
        // ----------------------------
        file.MainCategory = MediaMainCategory.Review;
        file.SubCategory = MediaSubCategory.Unknown;
    }
}