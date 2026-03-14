using ITMartinFileSorter.Domain.Entities;
using System.Security.Cryptography;

namespace ITMartinFileSorter.Application.Helpers;

public static class DuplicateHelper
{
    private static readonly Dictionary<string, List<string>> HashCache = new();

    public static string ComputeFileHash(string filePath)
    {
        if (!File.Exists(filePath)) return string.Empty;

        using var sha = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha.ComputeHash(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    public static bool IsDuplicate(MediaFile file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.FullPath)) return false;

        if (string.IsNullOrWhiteSpace(file.Hash))
        {
            try { file.SetHash(ComputeFileHash(file.FullPath)); }
            catch { return false; }
        }

        if (string.IsNullOrWhiteSpace(file.Hash)) return false;

        lock (HashCache)
        {
            if (!HashCache.ContainsKey(file.Hash))
            {
                HashCache[file.Hash] = new List<string> { file.FullPath };
                return false;
            }
            else
            {
                HashCache[file.Hash].Add(file.FullPath);
                return true;
            }
        }
    }

    public static void ResetCache()
    {
        lock (HashCache) HashCache.Clear();
    }
}