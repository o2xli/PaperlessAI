using OllamaSharp.Models;
using PaperlessAI.API.Adapter;
using PaperlessAI.Contracts;

namespace PaperlessAI.API.Services;

public class AiService : OllamaAdapter
{
    public AiService(IHttpClientFactory httpClientFactory, ILogger<AiService> logger) : base(httpClientFactory,
        model: "llama3.1:8b",
        temperature: 1f,
        topP: 0.5f, 
        seed: 101)
    {
        Logger = logger;
    }

    public ILogger<AiService> Logger { get; }

    public async Task<AiSchemas.DocumentMetaData?> GetDocumentMetaData(string contentText)
    {
        try
        {
            var response = await Call(contentText);
            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error calling AI service");
        }

        return default;
    }
}