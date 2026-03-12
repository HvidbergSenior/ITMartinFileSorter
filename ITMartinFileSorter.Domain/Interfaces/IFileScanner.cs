using ITMartinFileScanner.Domain.Entities;

namespace ITMartinFileScanner.Domain.Interfaces;

public interface IFileScanner
{
    IEnumerable<MediaFile> ScanFolder(string path);
}