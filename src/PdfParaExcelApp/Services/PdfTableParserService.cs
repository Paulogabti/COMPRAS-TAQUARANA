using PdfParaExcelApp.Models;
using PdfParaExcelApp.Parsers;

namespace PdfParaExcelApp.Services;

public class PdfTableParserService(IPdfTableParser parser) : IPdfTableParserService
{
    public Task<ParsedTableModel> ParseAsync(
        string pdfPath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
        => Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report("Preparando parser de tabela...");

            var parsed = parser.Parse(pdfPath, progress);
            if (parsed.Columns.Count == 0)
            {
                throw new InvalidOperationException("Tabela encontrada sem colunas detectadas.");
            }

            return parsed;
        }, cancellationToken);
}
