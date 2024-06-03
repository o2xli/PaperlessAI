// See https://aka.ms/new-console-template for more information
using System.Security.Principal;

PaperlessAI.Shared.FileStoreClient client = new("http://localhost:9082");

foreach (var file in System.IO.Directory.GetFiles("C:\\temp\\export", "*-archive.pdf"))
{
    await client.UploadFileAsync(file);
    Console.WriteLine($"Uploaded {file}");
}
