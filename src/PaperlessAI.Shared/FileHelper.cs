using System.Security.Cryptography;
using System.Text;

namespace PaperlessAI.Shared;

public static class FileHelper
{
    public static string GetHashFromPath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);

        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(fileName));

        var fileHash = BitConverter.ToString(bytes).Replace("-", "");
        return fileHash;
    }
}