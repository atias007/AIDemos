using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace MyFirstRag.Extractors;

internal static class DocxExtractor
{
    public static string ExtractTextFromDocx(string filePath)
    {
        var text = new StringBuilder();

        using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
        {
            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null) { return string.Empty; }
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                text.AppendLine(paragraph.InnerText);
            }
        }

        return text.ToString();
    }
}