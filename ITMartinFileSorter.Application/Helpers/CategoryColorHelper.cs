using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Helpers;

public static class CategoryColorHelper
{
    public static string GetColor(MediaMainCategory category)
    {
        return category switch
        {
            MediaMainCategory.Image => "category-image",
            MediaMainCategory.Video => "category-video",
            MediaMainCategory.Audio => "category-audio",
            MediaMainCategory.Document => "category-document",
            _ => "category-default"
        };
    }

    public static string GetIcon(MediaMainCategory category)
    {
        return category switch
        {
            MediaMainCategory.Image => "🖼",
            MediaMainCategory.Video => "🎬",
            MediaMainCategory.Audio => "🎵",
            MediaMainCategory.Document => "📄",
            _ => "📁"
        };
    }
}