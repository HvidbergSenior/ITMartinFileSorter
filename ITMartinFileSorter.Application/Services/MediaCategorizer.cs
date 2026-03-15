using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;
using ITMartinFileSorter.Application.Helpers;
using System.IO;

namespace ITMartinFileSorter.Application.Services;

public class MediaCategorizer
{
    public void Categorize(MediaFile file)
    {
        string ext = Path.GetExtension(file.FullPath).ToLowerInvariant();
        string fileName = Path.GetFileName(file.FullPath).ToLowerInvariant();

        string yearMonth = file.CreatedAt.Year > 1990 ? file.CreatedAt.ToString("yyyy-MM") : "Unknown";
        string yearOnly = file.CreatedAt.Year > 1990 ? file.CreatedAt.Year.ToString() : "Unknown";

        // ----------------------------
        // Images
        // ----------------------------
        if (file.Type == MediaType.Image)
        {
            // Screenshot detection
            if (ext == ".heic" && fileName.Contains("img"))
                file.SubCategory = MediaSubCategory.PhoneScreenshot;
            else if (fileName.Contains("screenshot"))
                file.SubCategory = MediaSubCategory.Screenshot;
            else
                file.SubCategory = MediaSubCategory.UnknownImage;

            // Device detection via EXIF
            var meta = ExifHelper.ReadMetadata(file.FullPath);
            if (meta.HasValue)
            {
                var (make, model, _) = meta.Value;
                if ((make?.Contains("Apple") ?? false) || (model?.Contains("iPhone") ?? false))
                {
                    file.DeviceModel = "iPhone";
                    file.TertiaryCategory = MediaTertiaryCategory.iPhone;
                }
                else
                {
                    file.DeviceModel = "Android";
                    file.TertiaryCategory = MediaTertiaryCategory.Android;
                }
            }

            // Location detection
            var coords = GpsHelper.GetCoordinates(file.FullPath);
            string locationFolder = "UnknownLocation";
            if (coords.HasValue)
            {
                var (lat, lng) = coords.Value;
                if (LocationFilter.IsInAarhus(lat, lng)) locationFolder = "RegionMidtjylland";
                else if (LocationFilter.IsInSjaelland(lat, lng)) locationFolder = "Sjaelland";
                else if (LocationFilter.IsInJutlandMinusAarhus(lat, lng)) locationFolder = "Jylland";
                else locationFolder = "UdenforDenmark";
            }

            file.Location = locationFolder;
            file.DynamicFolder = Path.Combine("Images", yearMonth, locationFolder);
            return;
        }

        // ----------------------------
        // Video
        // ----------------------------
        if (file.Type == MediaType.Video)
        {
            file.SubCategory = MediaSubCategory.UnknownVideo;

            var meta = VideoMetadataHelper.ReadMetadata(file.FullPath);
            if (meta.HasValue)
            {
                if (!string.IsNullOrEmpty(meta.Value.Model) && meta.Value.Model.Contains("iPhone"))
                    file.TertiaryCategory = MediaTertiaryCategory.iPhone;
                else
                    file.TertiaryCategory = MediaTertiaryCategory.UnknownDevice;

                if (meta.Value.Created.HasValue)
                    file.CreatedAt = meta.Value.Created.Value;
            }

            file.DynamicFolder = Path.Combine("Videos", yearMonth);
            return;
        }

        // ----------------------------
        // Audio
        // ----------------------------
        if (file.Type == MediaType.Audio)
        {
            file.SubCategory = MediaSubCategory.UnknownAudio;
            file.DynamicFolder = Path.Combine("Music", yearOnly);
            return;
        }

        // ----------------------------
        // Documents
        // ----------------------------
        if (file.Type == MediaType.Document)
        {
            string extFolder = ext.TrimStart('.').ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(extFolder)) extFolder = "Unknown";

            file.SubCategory = extFolder switch
            {
                "PDF" => MediaSubCategory.Pdf,
                "DOC" or "DOCX" => MediaSubCategory.Word,
                "XLS" or "XLSX" => MediaSubCategory.Excel,
                "TXT" => MediaSubCategory.Text,
                "PPT" or "PPTX" => MediaSubCategory.Presentation,
                _ => MediaSubCategory.UnknownDocument
            };

            file.DynamicFolder = Path.Combine("Documents", extFolder);
            return;
        }
    }
}