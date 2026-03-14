namespace ITMartinFileSorter.Application.Models;

public class DuplicateStats
{
    public int Count { get; set; }

    public long TotalSize { get; set; }

    public string TotalSizeReadable
    {
        get
        {
            double size = TotalSize;
            string[] units = { "B", "KB", "MB", "GB", "TB" };

            int i = 0;

            while (size >= 1024 && i < units.Length - 1)
            {
                size /= 1024;
                i++;
            }

            return $"{size:0.##} {units[i]}";
        }
    }
}