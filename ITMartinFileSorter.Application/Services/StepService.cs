using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Services;

public class StepService
{
    private readonly DuplicateService _duplicateService;

    public StepService(DuplicateService duplicateService)
    {
        _duplicateService = duplicateService;
    }

    public bool HasAudio => _duplicateService.AllFiles.Any(f => f.MainCategory == MediaMainCategory.Audio);
    public bool HasDocuments => _duplicateService.AllFiles.Any(f => f.MainCategory == MediaMainCategory.Document);
    public bool HasImages => _duplicateService.AllFiles.Any(f => f.MainCategory == MediaMainCategory.Image);
    public bool HasVideos => _duplicateService.AllFiles.Any(f => f.MainCategory == MediaMainCategory.Video);

    public string GetNextStep(string currentStep)
    {
        return currentStep switch
        {
            "Duplicates" => HasAudio ? "Audio" : HasDocuments ? "Documents" : HasImages ? "Images" : HasVideos ? "Videos" : "Done",
            "Audio" => HasDocuments ? "Documents" : HasImages ? "Images" : HasVideos ? "Videos" : "Done",
            "Documents" => HasImages ? "Images" : HasVideos ? "Videos" : "Done",
            "Images" => HasVideos ? "Videos" : "Done",
            _ => "Done"
        };
    }
}