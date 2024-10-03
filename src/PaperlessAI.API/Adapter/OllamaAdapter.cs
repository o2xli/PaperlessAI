using System.Text.Json;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using PaperlessAI.Contracts;

namespace PaperlessAI.API.Adapter;

public class OllamaAdapter : IAiAdapter
{
    private readonly IHttpClientFactory httpClientFactory;
    
    private readonly int seed;
    private readonly float temperature;
    private readonly float topP;
    private readonly string systemMessage = $$"""
             # IDENTITÄT und ZWECK
          
             Sie sind Experte für Dokumentenklassifizierung, auch Textklassifikation genannt, d.h. Sie erfassen und anylysieren 
             die in den verschiedenen Dokumenten enthaltenen Informationen automatisch und geben diese in den vordefinierten Kategorien aus.
          
             # EINGABE
          
             Der Benutzer wird ihnen als Eingabe  einen unstrukturierten Text geben, welcher mit OCR-Technologie aus einem gescannten Dokument extrahiert wurde.
          
             # AUSGABE ANWEISUNGEN
          
             - Geben Sie keine Warnungen oder Hinweise aus, sondern nur die gewünschten Abschnitte.
             - Geben Sie keine zusätzlichen Informationen aus.

          """;
    public OllamaAdapter(IHttpClientFactory httpClientFactory, string model, float temperature, float topP, int seed)
    {
        this.httpClientFactory = httpClientFactory;
        this.temperature = temperature;
        this.topP = topP;
        this.seed = seed;
    }

    public float? Temperature => temperature;


    public async Task<AiSchemas.DocumentMetaData> Call(string userInput)
    {
        var dict = await CallBase(userInput);
        if (dict is null)
            return null;
        
        var result = new AiSchemas.DocumentMetaData()
        {
            Correspondents = dict.AsArray("Correspondents"),
            DocumentType = dict.AsString("DocumentType"),
            Currency = dict.AsString("Currency"),
            TotalAmount = dict.AsString("TotalAmount"),
            CreationDate = dict.AsDateTime("CreationDate"),
            IBAN = dict.AsString("IBAN"),
            BIC = dict.AsString("BIC"),
            Keywords = dict.AsArray("Keywords"),
            Persons = dict.AsArray("Persons"),
            Subject = dict.AsString("Subject"),
            FileName = dict.AsString("FileName"),
        };
        
        return result;
    }

    private async Task<Dictionary<string,string>?> CallBase(string userInput)
    {
        var uri = new Uri("http://localhost:11434");
        var ollama = new OllamaApiClient(uri);

        var request = CreateModelRequest(userInput, "llama3.1:70b");
        var json = JsonSerializer.Serialize(request);
        var response = await ollama.Chat(request).StreamToEnd();
        var arguments = response?.Message.ToolCalls?.FirstOrDefault()?.Function?.Arguments;
        //if (arguments is null)
        //{
        //    response = await ollama.Chat(CreateModelRequest(userInput, "firefunction-v2"));
        //    arguments = response?.Message.ToolCalls?.FirstOrDefault()?.Function?.Arguments;
        //}
        return arguments;
    }
    
    private ChatRequest CreateModelRequest(string userInput, string model)
    {
       var request = new ChatRequest
       {
            Model = model,
            Format = "json",
            Stream = false,
            Options = new()
            {
                Temperature = temperature,
                Seed = 101,
                TopP = 0.5f,
            },
            
            Messages = new List<Message>()
            {
                new()
                {
                    Role = ChatRole.System,
                    Content = systemMessage,
                },
                new()
                {
                    Role = ChatRole.User,
                    Content = userInput,
                },
            },
            Tools = new List<Tool>()
            {
                new()
                {
                    Type = "function",
                    Function = new()
                    {
                        Name = "extract-meta-data",
                        Description = "Extrahiert Metadaten aus dem Text eines gescannten Dokuments.",
                        Parameters = new()
                        {
                           Type = "object",
                           Properties = new Dictionary<string, Properties>
                           {
                                {"Correspondents",  new() {Type = "string",Description = "Die Personen, Institutionen oder Firmen, von der ein Dokument stammt oder an die es gesendet wird. Mehrere Ergebnisse sind durch Komma getrennt."}},
                                {"Subject", new() {Type = "string", Description = "Das Subject eines Dokuments bezieht sich auf die Fragen, die das Dokument für die Nutzer beantworten kann."}},
                                {"FileName", new() {Type = "string", Description = "Als Kurzname besed auf dem Subject, um es als Dateiname ohne Dateityp-Erweiterung zu verwenden. Der Dateiname sollte mit dem aktuellen Datum beginnen, um ihn eindeutig zu machen. Verwenden Sie das folgende Datums-Zeit-Format: yyyyMMddHHmm."}},
                                {"Keywords", new() {Type = "string", Description = "Keywords sind themenbezogene Schlüsselwörter, die den wesentlichen Inhalt einer Aussage beschreiben. Sie liefern die für die Suche notwendigen Informationen. Mehrere Ergebnisse sind durch Komma getrennt."}},
                                {"CreationDate", new() {Type = "string", Description = "Ein Erstellungsdatum des Dokuments im ISO 8601-Format. Falls nicht gefunden, leer lassen."}},
                                {"IBAN", new() {Type = "string"}},
                                {"BIC", new() {Type = "string"}},
                                {"TotalAmount", new() {Type = "string"}},
                                {"Currency", new () {Type = "string"}},
                                {"Persons", new() {Type = "string", Description = "Eine Liste von Personen welche in den Dokument gefunden wurden. Mehrere Ergebnisse sind durch Komma getrennt."}},
                                {"DocumentType", new Properties{Type="string", Description = "Der Dokumententyp ist eine Kategorie, in die Dokumente basierend auf ihrem Inhalt, ihrer Funktion und ihrem Verwendungszweck eingeordnet werden.",Enum = ["Bankdokument","Steuer","Vertrag","Rechnung","Bescheinigung","Rente", "Wahl", "Brief", "Arztbericht","Zeugniss","Mahnung","Zahlungserinnerung","Versicherung","Kontoauszug","Mietvertrag","Handyvertrag", "Kreditkartenabrechnungen","Gehaltsabrechnung", "Finanzdokument"] } }
                           },
                           Required = new List<string> { "Correspondents", "Subject", "FileName", "Keywords", "CreationDate", "IBAN", "BIC", "TotalAmount", "Currency", "Persons" }
                       }
                    }
                },
            },
            
        };

        return request;

    }
}