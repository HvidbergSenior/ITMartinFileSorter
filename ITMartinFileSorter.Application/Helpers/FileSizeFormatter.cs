using ITMartinFileSorter.Domain.Entities;
using ITMartinFileSorter.Domain.Enums;

namespace ITMartinFileSorter.Application.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using ITMartinFileSorter.Domain.Interfaces;

    public static class FileSizeFormatter
    {
        // Format a single byte value
        public static string FormatCountSize(long bytes)
        {
            if (bytes > 1_000_000_000) return $"{bytes / 1_000_000_000.0:F2} GB";
            if (bytes > 1_000_000) return $"{bytes / 1_000_000.0:F2} MB";
            if (bytes > 1_000) return $"{bytes / 1_000.0:F2} KB";
            return $"{bytes} B";
        }

        // Format a list of MediaFiles for a specific type
        public static string FormatCountSize(IEnumerable<MediaFile> files, MediaType type)
        {
            var filtered = files.Where(f => f.Type == type);
            var totalBytes = filtered.Sum(f => f.SizeBytes);
            var count = filtered.Count();
            return $"{count} ({FormatCountSize(totalBytes)})";
        }
    }
}