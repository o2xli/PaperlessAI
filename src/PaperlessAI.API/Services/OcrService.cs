using Microsoft.Extensions.Logging;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PaperlessAI.API.Services;

public class OcrService
{
    public static string ExtractTextFromPDF(string pdfPath)
    {
        var text = new StringBuilder();
        using PdfDocument document = PdfDocument.Open(pdfPath);
        foreach (Page page in document.GetPages())
        {
            text.AppendLine(page.Text);
        }
        return text.ToString();
    }
}
