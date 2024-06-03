using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PaperlessAI.Shared;

public static class FileHelper
{
    public static string GetHashFromPath(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        using SHA256 sha256Hash = SHA256.Create();
        byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(fileName));
    
        string fileHash = BitConverter.ToString(bytes).Replace("-", ""); 
        return fileHash;
    }
}
