using CrucibleBugTracker.Enums;

namespace CrucibleBugTracker.Services.Interfaces
{
    public interface IBTFileService
    {
        public string ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage);

        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes);
    }
}
