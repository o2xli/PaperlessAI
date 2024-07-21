using Newtonsoft.Json.Linq;
using NJsonSchema;
using PaperlessAI.Contracts;

namespace PaperlessAI.Shared;

public static class DataExtensions
{
    public static bool IsPdf(this Data.Document document)
    {
        var fileName = document.OriginalFileName ?? document.FileName;
        return fileName!.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
    }

    public static string ToReducedJson(this JsonSchema schema)
    {
        var jObject = JObject.Parse(schema.ToJson());
        var reducedJObject = jObject["properties"];

        return reducedJObject.ToString();
    }
}