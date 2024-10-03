using System.Globalization;

namespace PaperlessAI.API;

public static class Extentions
{
    public static string[] AsArray(this Dictionary<string, string> dict, string key)
    {
        if (!dict.ContainsKey(key) || string.IsNullOrWhiteSpace(dict[key])) return [];
        return dict[key].Contains(',') ? dict[key].Split(',').Select(s => s.Trim()).ToArray() : [dict[key]];
    }
    
    public static DateTime? AsDateTime(this Dictionary<string, string> dict, string key)
    {
        if (!dict.ContainsKey(key) || string.IsNullOrWhiteSpace(dict[key])) return null;
        if (DateTime.TryParse(dict[key], new CultureInfo("en-us"), out var dt))
            return dt;

        return null;
    }

    public static string? AsString(this Dictionary<string, string> dict, string key)
    {
        if (!dict.ContainsKey(key) || string.IsNullOrWhiteSpace(dict[key])) return null;
        return dict[key];
    }

    
    
}