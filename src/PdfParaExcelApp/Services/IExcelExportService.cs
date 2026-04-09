using PdfParaExcelApp.Models;

namespace PdfParaExcelApp.Services;

public interface IExcelExportService
{
    Task ExportAsync(ParsedTableModel table, IReadOnlyList<ColumnDefinition> selectedColumns, string outputPath, CancellationToken cancellationToken = default);
}
