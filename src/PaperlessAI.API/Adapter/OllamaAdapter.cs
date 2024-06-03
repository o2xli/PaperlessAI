using Newtonsoft.Json;
using NJsonSchema;
using PaperlessAI.Shared;

namespace PaperlessAI.API.Adapter;

public class OllamaAdapter<T> : IAiAdapter<T>
{
    private readonly float temperature;
    private readonly float topP;
    private readonly int seed;
    private readonly string assistantMessageTemplate;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly string model;

    public float? Temperature => temperature;

    public OllamaAdapter(IHttpClientFactory httpClientFactory, string model, float temperature, float topP, int seed)
    {
        this.httpClientFactory = httpClientFactory;
        this.model = model;
        this.temperature = temperature;
        this.topP = topP;
        this.seed = seed;
    }

    public async Task<string> CallRaw(string userInput, string jsonSchema)
    {
        string systemMessage = $$"""
                Sie sind ein Dienst, der Benutzer anfragen zu einem eingescannten Dokument im JSON Format des Typs "{{typeof(T).Name}}" gemäß den folgenden JSON-Schemadefinitionen übersetzt:
                ```    
                    {{jsonSchema}}
                ```
                
                Ihre Aufgabe ist es, Metadaten aus dem Text eines gescannten Dokuments zu extrahieren.
                """;

        string userMessage = $$"""
                Erkenne in dem folgenden Text die felder aus JSON-Schemadefinitionen und erstelle passend dazu ein JSON Object :


                \"{{userInput}}\"                        
                """;

        string resultContent = await CallAPI(systemMessage, userMessage);

        return resultContent;
    }



    private async Task<string> TryFixJsonErrors(string jsonData, string jsonSchema, string[] errors)
    {
        string systemMessage = $$"""
                Sie sind ein Dienst, welcher fehlerhafte JSON Daten korrigiert. Als referenz dient das JSON-Schema:
                ```    
                    {{jsonSchema}}
                ```                
                """;

        string userMessage = $$"""
                Die JSON-Daten enhalten die Fehler:
                \"{{string.Join(", ", errors)}}\"
                Bitte korrigieren Sie die JSON-Daten:
                \"{{jsonData}}\"                        
                """;

        string resultContent = await CallAPI(systemMessage, userMessage);

        return resultContent;
    }



    public async Task<T> Call(string userInput)
    {
        string resultContent = await CallBase(userInput);

        T? result = JsonConvert.DeserializeObject<T>(resultContent);

        return result;
    }

    protected async Task<string> CallAPI(string systemMessage, string userMessage)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("Ollama");

        OllamaRequest request = new(model, new Message[]
        {
                new("system", systemMessage),
                //new Message("assistant", assistantMessage),
                new("user", userMessage)
        },
        new Options(seed, temperature, topP));

        string combo = systemMessage + userMessage;

        HttpResponseMessage responseMessage = await httpClient.PostAsJsonAsync("/api/chat", request);

        OllamaResponse? chatResult = await responseMessage.Content.ReadFromJsonAsync<OllamaResponse>();

        Message resultMessage = chatResult.message;

        string resultContent = ExtractJsonCodeMarkDown(resultMessage.content);

        return resultContent;
    }



    protected static string[] GetJsonValidationErrors(JsonSchema jsonSchema, string resultContent)
    {
        return jsonSchema.Validate(resultContent)
        .Where(e => e.Kind != NJsonSchema.Validation.ValidationErrorKind.StringTooShort &&
                e.Kind != NJsonSchema.Validation.ValidationErrorKind.NumberExpected &&
                e.Kind != NJsonSchema.Validation.ValidationErrorKind.PropertyRequired)
        .Select(e => e.ToString())
        .ToArray();
    }

    protected async Task<string> CallBase(string userInput)
    {
        JsonSchema jsonSchema = JsonSchema.FromType(typeof(T));

        string resultContent = await CallRaw(userInput, jsonSchema.ToReducedJson());

        resultContent = ExtractJsonCodeMarkDown(resultContent);

        string[] errors = GetJsonValidationErrors(jsonSchema, resultContent);

        if (errors.Any())
        {
            resultContent = await TryFixJsonErrors(resultContent, jsonSchema.ToReducedJson(), errors);
        }

        return resultContent;
    }

    protected static string ExtractJsonCodeMarkDown(string input)
    {
        string result = input;
        if (result.Contains("```json"))
        {
            string startTag = "```json";
            string endTag = "```";
            int startIndex = result.IndexOf(startTag) + startTag.Length;
            int endIndex = result.IndexOf(endTag, startIndex);
            result = result.Substring(startIndex, endIndex - startIndex);
        }

        return result.Trim('\n');
    }

    private record OllamaRequest(string model, Message[] messages, Options options, bool stream = false, string format = "json");
    private record OllamaResponse(string model,
                                  string created_at,
                                  Message message,
                                  bool done,
                                  Int64 otal_duration,
                                  Int64 load_duration,
                                  int prompt_eval_count,
                                  Int64 prompt_eval_duration,
                                  Int64 eval_count,
                                  Int64 eval_duration);
    private record Message(string role, string content);

    private record Options(int seed, float temperature, float top_p);
}
