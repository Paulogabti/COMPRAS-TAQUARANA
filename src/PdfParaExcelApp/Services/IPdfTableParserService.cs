using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public interface IPdfTableParserService
{
    Task<ParsedTableModel> ParseAsync(string pdfPath, IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}
