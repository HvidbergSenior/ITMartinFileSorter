namespace ITMartinFileSorter.Domain.Enums
{
    // Top-level categories
    public enum MediaMainCategory
    {
        Audio = 0,
        Video = 1,
        Document = 2,
        Image = 3
    }

    // Type-specific subcategories
    public enum MediaSubCategory
    {
        Duplicate,  // secondary flag for duplicates

        // Audio
        Music,
        Podcast,
        VoiceMemo,
        UnknownAudio,

        // Video
        Movie,
        Clip,
        Tutorial,
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
        iPhoneScreenshot,
        AndroidScreenshot,
        Edited,
        UnknownImage
    }

    // Tertiary categories: finer classification
    public enum MediaTertiaryCategory
    {
        // Location
        RegionMidtjylland,
        Jutland,
        Sjaelland,
        OutsideDenmark,
        UnknownLocation,

        // Device
        iPhone,
        Android,
        DSLR,
        GoPro,
        UnknownDevice,

        // Duplicate
        Duplicate,

        // Other
        Edited,
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