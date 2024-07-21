namespace PaperlessAI.Web;

public static class Extensions
{
    public static string ToDeString(this DateTime? dateTime)
    {
        return !dateTime.HasValue ? string.Empty : dateTime!.Value.ToString("dd'.'MM'.'yyyy");
    }
}