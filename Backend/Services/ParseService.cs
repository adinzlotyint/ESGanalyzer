using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ESGanalyzer.Backend.Services {
    public class ParseService : IParseService {
        public async Task<string> ExtractTextAsync(IFormFile file) {
            using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            ms.Seek(0, SeekOrigin.Begin);

            using var doc = WordprocessingDocument.Open(ms, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            if (body == null) return string.Empty;

            var paragraphs = body.Descendants<Paragraph>();
            var text = string.Join(Environment.NewLine, paragraphs.Select(p => p.InnerText));

            return text;
        }
    }
}
