using System.Text;
using UglyToad.PdfPig;

namespace PaperlessAI.API.Services;

public class OcrService
{
    public static string ExtractTextFromPDF(string pdfPath)
    {
        var text = new StringBuilder();
        using var document = PdfDocument.Open(pdfPath);
        foreach (var page in document.GetPages()) text.AppendLine(page.Text);
        return text.ToString();
    }
}