using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace MyFirstRag.Extractors;

internal class PdfExtractor
{
    // Extract text from PDF using PdfPig
    public static string ExtractTextFromPdf(string pdfPath)
    {
        var text = new StringBuilder();

        using PdfDocument document = PdfDocument.Open(pdfPath);
        foreach (var page in document.GetPages())
        {
            var pageText = ContentOrderTextExtractor.GetText(page);
            text.AppendLine(pageText);
        }

        return text.ToString();
    }
}