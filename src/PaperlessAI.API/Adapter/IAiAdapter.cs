using PaperlessAI.Contracts;

namespace PaperlessAI.API.Adapter;

public interface IAiAdapter
{
    float? Temperature { get; }
    Task<AiSchemas.DocumentMetaData> Call(string userInput);
}