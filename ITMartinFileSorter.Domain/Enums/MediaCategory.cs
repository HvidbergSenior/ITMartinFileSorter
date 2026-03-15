namespace ITMartinFileSorter.Domain.Enums
{
    // Top-level categories
    public enum MediaMainCategory
    {
        Audio = 0,
        Video = 1,
        Document = 2,
        Image = 3,
        Duplicate
        
    }

    // Type-specific subcategories
    public enum MediaSubCategory
    {

        // Audio
        Music,
        VoiceMemo,
        UnknownAudio,

        // Video
        Movie,
        Clip,
        ScreenRecording,
        UnknownVideo,

        // Document
        Pdf,
        Word,
        Excel,
        Text,
        Presentation,
        UnknownDocument,

        // Image
        Screenshot,
        PhoneScreenshot,
        UnknownImage
    }

    // Tertiary categories: finer classification
    public enum MediaTertiaryCategory
    {
        // Location
        RegionMidtjylland,
        Jylland,
        Sjaelland,
        UdenforDenmark,
        UnknownLocation,

        // Device
        iPhone,
        Android,
        UnknownDevice,

        // Other
        Unknown
    }

    // File type
    public enum MediaType
    {
        Audio,
        Video,
        Document,
        Image
    }
}