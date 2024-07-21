namespace PaperlessAI.API.Adapter;

public interface IAiAdapter<T>
{
    float? Temperature { get; }
    Task<T> Call(string userInput);
}