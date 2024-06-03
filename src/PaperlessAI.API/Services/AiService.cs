using PaperlessAI.API.Adapter;
using PaperlessAI.Contracts;

namespace PaperlessAI.API.Services;

public class AiService : OllamaAdapter<AiSchemas.DocumentMetaData>
{
    public AiService(IHttpClientFactory httpClientFactory,ILogger<AiService> logger) : base(httpClientFactory, "llama3:8b", 0.4f, 0.5f, 101)
    {
        Logger = logger;
    }

    public ILogger<AiService> Logger { get; }

    public async Task<AiSchemas.DocumentMetaData?> GetDocumentMetaData(string contentText)
    {
        try
        {
            var response = await base.Call(contentText);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling AI service");
        }
        return default;
    }
}
