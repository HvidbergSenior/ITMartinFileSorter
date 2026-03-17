using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;
using ITMartinFileSorter.Application.Helpers;

namespace ITMartinFileSorter.Application.Services;

public class MediaCategorizer
{
    public void Categorize(MediaFile file)
    {
        string ext = file.Extension.ToLowerInvariant();
        string fileName = file.FileName.ToLowerInvariant();

        string yearMonth = file.Year > 1990
            ? $"{file.Year}-{file.Month:00}"
            : "Unknown";

        string yearOnly = file.Year > 1990
            ? file.Year.ToString()
            : "Unknown";

        // ------------------------------------------------
        // IMAGES
        // ------------------------------------------------
        if (file.Type == MediaType.Image)
        {
            // --- Screenshot / camera detection ---
            if (fileName.Contains("screenshot"))
                file.SubCategory = MediaSubCategory.Screenshot;

            else if (ext is ".heic" or ".heif")
                file.SubCategory = MediaSubCategory.PhonePhoto;

            else if (fileName.StartsWith("img_") || fileName.StartsWith("dsc_"))
                file.SubCategory = MediaSubCategory.Camera;

            else
                file.SubCategory = MediaSubCategory.OtherImage;

            // --- Device detection via EXIF ---
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

            // --- GPS location detection ---
            var coords = GpsHelper.GetCoordinates(file.FullPath);

            MediaTertiaryCategory locationCategory = MediaTertiaryCategory.UnknownLocation;

            if (coords.HasValue)
            {
                var (lat, lng) = coords.Value;

                if (LocationFilter.IsInAarhus(lat, lng))
                    locationCategory = MediaTertiaryCategory.RegionMidtjylland;

                else if (LocationFilter.IsInSjaelland(lat, lng))
                    locationCategory = MediaTertiaryCategory.Sjaelland;

                else if (LocationFilter.IsInJutlandMinusAarhus(lat, lng))
                    locationCategory = MediaTertiaryCategory.Jylland;

                else
                    locationCategory = MediaTertiaryCategory.UdenforDenmark;
            }

            file.Location = locationCategory.ToString();

            file.DynamicFolder = Path.Combine(
                "Images",
                file.SubCategory.ToString(),
                yearMonth,
                locationCategory.ToString()
            );

            return;
        }

        // ------------------------------------------------
        // VIDEOS
        // ------------------------------------------------
        if (file.Type == MediaType.Video)
        {
            if (fileName.Contains("screen"))
                file.SubCategory = MediaSubCategory.ScreenRecording;
            else
                file.SubCategory = MediaSubCategory.OtherVideo;

            var meta = VideoMetadataHelper.ReadMetadata(file.FullPath);

            if (meta.HasValue)
            {
                if (!string.IsNullOrEmpty(meta.Value.Model) &&
                    meta.Value.Model.Contains("iPhone"))
                {
                    file.TertiaryCategory = MediaTertiaryCategory.iPhone;
                }
                else
                {
                    file.TertiaryCategory = MediaTertiaryCategory.UnknownDevice;
                }

                if (meta.Value.Created.HasValue)
                {
                    file.CreatedAt = meta.Value.Created.Value;
                    file.Year = file.CreatedAt.Year;
                    file.Month = file.CreatedAt.Month;
                }
            }

            file.DynamicFolder = Path.Combine(
                "Videos",
                file.SubCategory.ToString(),
                yearMonth
            );

            return;
        }

        // ------------------------------------------------
        // AUDIO
        // ------------------------------------------------
        if (file.Type == MediaType.Audio)
        {
            if (ext is ".mp3" or ".flac")
                file.SubCategory = MediaSubCategory.Music;

            else if (ext is ".wav" or ".aac")
                file.SubCategory = MediaSubCategory.VoiceMemo;

            else
                file.SubCategory = MediaSubCategory.UnknownAudio;

            file.DynamicFolder = Path.Combine(
                "Music",
                yearOnly
            );

            return;
        }

        // ------------------------------------------------
        // DOCUMENTS
        // ------------------------------------------------
        if (file.Type == MediaType.Document)
        {
            file.SubCategory = ext switch
            {
                ".pdf" => MediaSubCategory.Pdf,
                ".doc" or ".docx" => MediaSubCategory.Word,
                ".xls" or ".xlsx" => MediaSubCategory.Excel,
                ".txt" => MediaSubCategory.Text,
                ".ppt" or ".pptx" => MediaSubCategory.Presentation,
                ".csv" => MediaSubCategory.Csv,
                _ => MediaSubCategory.UnknownDocument
            };

            file.DynamicFolder = Path.Combine(
                "Documents",
                file.SubCategory.ToString()
            );

            return;
        }
    }
}