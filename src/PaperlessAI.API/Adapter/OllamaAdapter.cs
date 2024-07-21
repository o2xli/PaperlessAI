using Newtonsoft.Json;
using NJsonSchema;
using NJsonSchema.Validation;
using PaperlessAI.Shared;

namespace PaperlessAI.API.Adapter;

public class OllamaAdapter<T> : IAiAdapter<T>
{
    private readonly string assistantMessageTemplate;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string model;
    private readonly int seed;
    private readonly float temperature;
    private readonly float topP;

    public OllamaAdapter(IHttpClientFactory httpClientFactory, string model, float temperature, float topP, int seed)
    {
        this.httpClientFactory = httpClientFactory;
        this.model = model;
        this.temperature = temperature;
        this.topP = topP;
        this.seed = seed;
    }

    public float? Temperature => temperature;


    public async Task<T> Call(string userInput)
    {
        var resultContent = await CallBase(userInput);

        var result = JsonConvert.DeserializeObject<T>(resultContent);

        return result;
    }

    public async Task<string> CallRaw(string userInput, string jsonSchema)
    {
        var systemMessage = $$"""
                              Sie sind ein Dienst, der Benutzer anfragen zu einem eingescannten Dokument im JSON Format des Typs "{{typeof(T).Name}}" gemäß den folgenden JSON-Schemadefinitionen übersetzt:
                              ```    
                                  {{jsonSchema}}
                              ```

                              Ihre Aufgabe ist es, Metadaten aus dem Text eines gescannten Dokuments zu extrahieren.
                              """;

        var userMessage = $$"""
                            Erkenne in dem folgenden Text die felder aus JSON-Schemadefinitionen und erstelle passend dazu ein JSON Object :


                            \"{{userInput}}\"                        
                            """;

        var resultContent = await CallAPI(systemMessage, userMessage);

        return resultContent;
    }


    private async Task<string> TryFixJsonErrors(string jsonData, string jsonSchema, string[] errors)
    {
        var systemMessage = $$"""
                              Sie sind ein Dienst, welcher fehlerhafte JSON Daten korrigiert. Als referenz dient das JSON-Schema:
                              ```    
                                  {{jsonSchema}}
                              ```                
                              """;

        var userMessage = $$"""
                            Die JSON-Daten enhalten die Fehler:
                            \"{{string.Join(", ", errors)}}\"
                            Bitte korrigieren Sie die JSON-Daten:
                            \"{{jsonData}}\"                        
                            """;

        var resultContent = await CallAPI(systemMessage, userMessage);

        return resultContent;
    }

    protected async Task<string> CallAPI(string systemMessage, string userMessage)
    {
        var httpClient = httpClientFactory.CreateClient("Ollama");

        OllamaRequest request = new(model, new Message[]
            {
                new("system", systemMessage),
                //new Message("assistant", assistantMessage),
                new("user", userMessage)
            },
            new Options(seed, temperature, topP));

        var combo = systemMessage + userMessage;

        var responseMessage = await httpClient.PostAsJsonAsync("/api/chat", request);

        var chatResult = await responseMessage.Content.ReadFromJsonAsync<OllamaResponse>();

        var resultMessage = chatResult.message;

        var resultContent = ExtractJsonCodeMarkDown(resultMessage.content);

        return resultContent;
    }


    protected static string[] GetJsonValidationErrors(JsonSchema jsonSchema, string resultContent)
    {
        return jsonSchema.Validate(resultContent)
            .Where(e => e.Kind != ValidationErrorKind.StringTooShort &&
                        e.Kind != ValidationErrorKind.NumberExpected &&
                        e.Kind != ValidationErrorKind.PropertyRequired)
            .Select(e => e.ToString())
            .ToArray();
    }

    protected async Task<string> CallBase(string userInput)
    {
        var jsonSchema = JsonSchema.FromType(typeof(T));

        var resultContent = await CallRaw(userInput, jsonSchema.ToReducedJson());

        resultContent = ExtractJsonCodeMarkDown(resultContent);

        var errors = GetJsonValidationErrors(jsonSchema, resultContent);

        if (errors.Any()) resultContent = await TryFixJsonErrors(resultContent, jsonSchema.ToReducedJson(), errors);

        return resultContent;
    }

    protected static string ExtractJsonCodeMarkDown(string input)
    {
        var result = input;
        if (result.Contains("```json"))
        {
            var startTag = "```json";
            var endTag = "```";
            var startIndex = result.IndexOf(startTag) + startTag.Length;
            var endIndex = result.IndexOf(endTag, startIndex);
            result = result.Substring(startIndex, endIndex - startIndex);
        }

        return result.Trim('\n');
    }

    private record OllamaRequest(
        string model,
        Message[] messages,
        Options options,
        bool stream = false,
        string format = "json");

    private record OllamaResponse(
        string model,
        string created_at,
        Message message,
        bool done,
        long otal_duration,
        long load_duration,
        int prompt_eval_count,
        long prompt_eval_duration,
        long eval_count,
        long eval_duration);

    private record Message(string role, string content);

    private record Options(int seed, float temperature, float top_p);
}