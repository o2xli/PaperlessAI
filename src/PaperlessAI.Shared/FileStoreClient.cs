namespace PaperlessAI.Shared;

public class FileStoreClient
{
    private readonly HttpClient _client;

    public FileStoreClient(string baseAddress)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseAddress) };
    }

    public async Task<HttpResponseMessage> UploadFileAsync(string filePath)
    {
        var content = new MultipartFormDataContent();
        var fileStream = new FileStream(filePath, FileMode.Open);
        content.Add(new StreamContent(fileStream), "file", Path.GetFileName(filePath));
        return await _client.PostAsync("/upload", content);
    }

    public async Task<HttpResponseMessage> DownloadFileAsync(string filename)
    {
        return await _client.GetAsync($"/download/{filename}");
    }
}