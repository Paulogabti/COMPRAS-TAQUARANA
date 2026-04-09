using PdfParaExcelApp.Models;
using PdfParaExcelApp.Parsers;

namespace PdfParaExcelApp.Services;

public class PdfTableParserService(IPdfTableParser parser) : IPdfTableParserService
{
    public Task<ParsedTableModel> ParseAsync(string pdfPath, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return parser.Parse(pdfPath, progress);
        }, cancellationToken);
}
