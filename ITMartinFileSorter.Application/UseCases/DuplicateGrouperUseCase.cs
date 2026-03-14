using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.UseCases;

public class DuplicateGrouperUseCase
{
    public Dictionary<MediaMainCategory, Dictionary<MediaSubCategory, List<MediaFile>>> GroupByCategory(
        Dictionary<string, List<MediaFile>> duplicates)
    {
        var grouped = new Dictionary<MediaMainCategory, Dictionary<MediaSubCategory, List<MediaFile>>>();

        foreach (var dupGroup in duplicates.Values)
        {
            foreach (var file in dupGroup)
            {
                if (!grouped.ContainsKey(file.MainCategory))
                    grouped[file.MainCategory] = new Dictionary<MediaSubCategory, List<MediaFile>>();

                var subDict = grouped[file.MainCategory];

                if (!subDict.ContainsKey(file.SubCategory))
                    subDict[file.SubCategory] = new List<MediaFile>();

                subDict[file.SubCategory].Add(file);
            }
        }

        return grouped;
    }
}