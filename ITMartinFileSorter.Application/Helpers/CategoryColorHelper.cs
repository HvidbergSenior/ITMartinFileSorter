using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Helpers;

public static class CategoryColorHelper
{
    public static string GetColor(MediaMainCategory category)
    {
        return category switch
        {
            MediaMainCategory.Image => "category-image",      // green
            MediaMainCategory.Video => "category-video",      // orange
            MediaMainCategory.Audio => "category-audio",      // blue
            MediaMainCategory.Document => "category-document",// purple
            _ => ""
        };
    }
}