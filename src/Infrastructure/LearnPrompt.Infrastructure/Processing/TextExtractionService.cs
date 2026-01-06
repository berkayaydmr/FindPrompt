using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using LearnPrompt.Application.Processing;
using UglyToad.PdfPig;

namespace LearnPrompt.Infrastructure.Processing
{
    public class TextExtractionService : ITextExtractionService
    {
        public async Task<string> ExtractTextAsync(string filePath, string fileName, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();

            return extension switch
            {
                ".txt" => await System.IO.File.ReadAllTextAsync(filePath, cancellationToken),
                ".pdf" => ExtractFromPdf(filePath),
                ".docx" => ExtractFromDocx(filePath),
                ".pptx" => ExtractFromPptx(filePath),
                _ => throw new NotSupportedException($"Unsupported file extension: {extension}")
            };
        }

        private static string ExtractFromPdf(string filePath)
        {
            var builder = new StringBuilder();

            using var document = PdfDocument.Open(filePath);

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }

        private static string ExtractFromDocx(string filePath)
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart?.Document?.Body;
            if (body == null) return string.Empty;

            var paragraphs = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                .Select(p => p.InnerText)
                .Where(text => !string.IsNullOrWhiteSpace(text));

            return string.Join(Environment.NewLine, paragraphs);
        }

        private static string ExtractFromPptx(string filePath)
        {
            using var presentation = PresentationDocument.Open(filePath, false);
            var slides = presentation.PresentationPart?.SlideParts;
            if (slides == null) return string.Empty;

            var builder = new StringBuilder();

            foreach (var slide in slides)
            {
                var texts = slide.Slide?.Descendants<DocumentFormat.OpenXml.Drawing.Text>()
                    .Select(t => t.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t)) ?? Enumerable.Empty<string>();

                foreach (var t in texts)
                {
                    builder.AppendLine(t);
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}

