namespace ITMartinFileSorter.Application.Helpers;

public static class EnumDisplayHelper
{
    public static string Format(Enum value)
    {
        return value.ToString()
            .Replace("UnknownImage", "Unknown Image")
            .Replace("UnknownVideo", "Unknown Video")
            .Replace("UnknownAudio", "Unknown Audio")
            .Replace("UnknownDocument", "Unknown Document")
            .Replace("ScreenRecording", "Screen Recording")
            .Replace("VoiceMemo", "Voice Memo")
            .Replace("iPhoneScreenshot", "iPhone Screenshot")
            .Replace("AndroidScreenshot", "Android Screenshot");
    }
}